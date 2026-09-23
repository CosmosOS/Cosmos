// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Pci;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// xHCI host controller driver (eXtensible Host Controller Interface 1.2),
/// the controller behind every USB port of a PC since ~2012 and of QEMU's
/// qemu-xhci. Device-class agnostic: it addresses devices, runs control
/// and bulk transfers, opens interrupt endpoints and registers hubs, and
/// leaves what the devices are to the <see cref="UsbDriver"/>s.
///
/// <para>Commands, control transfers and bulk transfers are synchronous
/// and only issued from thread context. Completions arrive on one event
/// ring, drained by the MSI-X handler, by the synchronous waits
/// themselves, and by <see cref="Poll"/> where MSI-X is unavailable.</para>
///
/// <para>Two locks, always taken in this order: <c>_eventLock</c> covers
/// the event ring and the state of the synchronous waits;
/// <c>_ringLock</c> covers the command ring and every transfer ring.
/// Interrupt handlers of class drivers run under the event lock only, so
/// they may queue transfers (keyboard LEDs) without deadlocking.</para>
///
/// <para>Root port changes raise a Port Status Change Event, which only
/// wakes <see cref="UsbManager"/>'s hot-plug thread (after making the
/// transfers of a device that left fail at once); the ports themselves are
/// handled on that thread, in <see cref="HandlePortChanges"/>.</para>
///
/// <para>Not implemented yet: isochronous endpoints, interrupt OUT
/// endpoints and streams.</para>
/// </summary>
internal sealed unsafe partial class XhciController : UsbHostController
{
    // USBCMD (xHCI 1.2 §5.4.1).
    private const uint UsbCmdRun = 1u << 0;
    private const uint UsbCmdReset = 1u << 1;
    private const uint UsbCmdInterrupterEnable = 1u << 2;

    // USBSTS (xHCI 1.2 §5.4.2).
    private const uint UsbStsHalted = 1u << 0;
    private const uint UsbStsEventInterrupt = 1u << 3;
    private const uint UsbStsControllerNotReady = 1u << 11;

    /// <summary>CRCR.RCS: the command ring's initial cycle state.</summary>
    private const ulong CrcrRingCycleState = 1;

    /// <summary>PAGESIZE bit 0: the controller supports 4 KiB pages.</summary>
    private const uint PageSize4KiB = 1;

    // IMAN (xHCI 1.2 §5.5.2.1). IP is RW1C.
    private const uint ImanInterruptPending = 1u << 0;
    private const uint ImanInterruptEnable = 1u << 1;

    /// <summary>IMOD interval in 250 ns units: 40 µs, Linux's default moderation.</summary>
    private const uint InterruptModerationInterval = 160;

    /// <summary>ERDP.EHB: Event Handler Busy, RW1C, cleared with every dequeue pointer update.</summary>
    private const ulong ErdpEventHandlerBusy = 1ul << 3;

    // Extended capability list (xHCI 1.2 §7).
    private const uint ExtendedCapabilityIdMask = 0xFF;
    private const int ExtendedCapabilityNextShift = 8;
    private const uint ExtendedCapabilityNextMask = 0xFF;
    private const int DwordShift = 2;
    private const byte LegacySupportCapabilityId = 1;

    // USB Legacy Support capability (xHCI 1.2 §7.1).
    private const uint LegacyBiosOwned = 1u << 16;
    private const uint LegacyOsOwned = 1u << 24;
    private const ulong LegacyControlStatusOffset = 4;

    /// <summary>USBLEGCTLSTS bits kept on write: the RsvdP ones. Every SMI enable is cleared (Linux XHCI_LEGACY_DISABLE_SMI).</summary>
    private const uint LegacyDisableSmiMask = (0x7u << 1) | (0xFFu << 5) | (0x7u << 17);

    /// <summary>USBLEGCTLSTS bits 31:29, RW1C SMI event flags.</summary>
    private const uint LegacySmiEvents = 0x7u << 29;

    private const uint FirmwareHandoffTimeoutMs = 1000;
    private const uint HaltTimeoutMs = 100;
    private const uint ResetTimeoutMs = 1000;

    /// <summary>
    /// Some controllers hang when their registers are read within 1 ms of
    /// setting HCRST; Linux's xhci_reset waits the same.
    /// </summary>
    private const uint ResetSettleMs = 1;

    private readonly PciDevice _pci;
    private readonly int _index;
    private readonly XhciRegisters _regs;
    private readonly XhciRing _commandRing;
    private readonly XhciEventRing _eventRing;
    private readonly ulong* _deviceContextArray;
    private readonly ulong _deviceContextArrayAddress;

    /// <summary>Addressed devices by slot ID (index 0 is unused: slot IDs start at 1).</summary>
    private readonly XhciDevice?[] _devices;

    /// <summary>USB major revision of each root port (index port - 1), from the Supported Protocol capabilities.</summary>
    private readonly byte[] _portMajorRevision;

    private SchedSpinLock _eventLock;
    private SchedSpinLock _ringLock;
    private bool _msiXEnabled;

    public XhciController(PciDevice pci, int index)
    {
        _pci = pci;
        _index = index;
        XhciDma.Initialize();
        pci.EnableMemory(true);
        pci.EnableBusMaster(true);

        ulong bar0Phys = pci.GetBar64Address(0);
        if (bar0Phys == 0)
        {
            throw new InvalidOperationException("[xHCI] BAR0 is not a memory BAR");
        }

        // ARM64 needs the register block mapped as Device memory before its
        // HHDM alias is dereferenceable; x64 needs it above 4 GiB only.
        PlatformHAL.Initializer?.EnsureMmioMapped(bar0Phys);
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        _regs = new XhciRegisters(bar0Phys + hhdmOffset);
        PlatformHAL.Initializer?.EnsureMmioMapped(bar0Phys + _regs.MappedLength - 1);

        _devices = new XhciDevice?[_regs.MaxSlots + 1];
        _portMajorRevision = new byte[_regs.MaxPorts];
        _commandRing = new XhciRing();
        _eventRing = new XhciEventRing();
        _deviceContextArray = (ulong*)XhciDma.AllocPages(1, out _deviceContextArrayAddress);
    }

    public override string Name => "xHCI";

    public override void Initialize()
    {
        Serial.WriteString("[xHCI] Controller #");
        Serial.WriteNumber((uint)_index);
        Serial.WriteString(": version 0x");
        Serial.WriteHex((uint)_regs.HciVersion);
        Serial.WriteString(", ");
        Serial.WriteNumber((uint)_regs.MaxSlots);
        Serial.WriteString(" slots, ");
        Serial.WriteNumber((uint)_regs.MaxPorts);
        Serial.WriteString(" ports, ");
        Serial.WriteNumber((uint)_regs.ContextSize);
        Serial.WriteString("-byte contexts, ");
        Serial.WriteNumber((uint)_regs.MaxScratchpadBuffers);
        Serial.WriteString(" scratchpad buffers\n");

        if ((_regs.PageSize & PageSize4KiB) == 0)
        {
            throw new InvalidOperationException("[xHCI] Controller does not support 4 KiB pages");
        }

        if (!_regs.Is64BitCapable)
        {
            Serial.WriteString("[xHCI] WARNING: controller only takes 32-bit DMA addresses\n");
        }

        TakeOwnershipFromFirmware();
        ReadSupportedProtocols();
        Reset();

        _regs.Config = _regs.MaxSlots;
        SetupScratchpad();
        _regs.Dcbaap = _deviceContextArrayAddress;
        _regs.Crcr = _commandRing.PhysicalAddress | CrcrRingCycleState;

        // ERSTBA is written last: it is what arms the event ring (xHCI 1.2 §4.9.4).
        _regs.Imod = InterruptModerationInterval;
        _regs.Erstsz = XhciEventRing.SegmentCount;
        _regs.Erdp = _eventRing.DequeuePointer;
        _regs.Erstba = _eventRing.SegmentTableAddress;

        SetupInterrupts();

        _regs.UsbCmd |= UsbCmdRun | (_msiXEnabled ? UsbCmdInterrupterEnable : 0);
        if (!WaitForStatus(UsbStsHalted, 0, HaltTimeoutMs))
        {
            throw new InvalidOperationException("[xHCI] Controller did not start");
        }

        Serial.WriteString(_msiXEnabled ? "[xHCI] Running, events via MSI-X\n" : "[xHCI] Running, events polled\n");
    }

    public override void Poll()
    {
        using (_eventLock.AcquireIrqSafe())
        {
            DrainEvents();
        }
    }

    public override bool IsPolled => !_msiXEnabled;

    public override void ReleaseDevice(UsbDevice device)
    {
        if (device is not XhciDevice xhciDevice || xhciDevice.HostController != this)
        {
            return;
        }

        // A transfer still waiting on the device gives up once it sees it
        // disconnected, then drops the lock it runs under: holding each of
        // those locks once guarantees none is left using what is freed below.
        xhciDevice.MarkDisconnected();
        _controlMutex.Acquire();
        _controlMutex.Release();
        xhciDevice.WaitForBulkTransfers();

        byte slotId = xhciDevice.SlotId;
        ExecuteCommand(0, XhciTrb.TypeField(XhciTrbType.DisableSlotCommand) | ((uint)slotId << XhciTrb.SlotIdShift), out _);

        // Past the event lock no event reaches the device's pipes, past the
        // ring lock no fire-and-forget transfer is mid-enqueue on its rings.
        using (_eventLock.AcquireIrqSafe())
        {
            using (_ringLock.AcquireIrqSafe())
            {
                _devices[slotId] = null;
                _deviceContextArray[slotId] = 0;
            }
        }

        xhciDevice.Free();
    }

    /// <summary>
    /// Asks the firmware to hand the controller over through the USB Legacy
    /// Support capability, and turns off the SMIs it may still raise for
    /// PS/2 emulation (xHCI 1.2 §4.22.1).
    /// </summary>
    private void TakeOwnershipFromFirmware()
    {
        ulong capability = FindExtendedCapability(LegacySupportCapabilityId, 0);
        if (capability == 0)
        {
            return;
        }

        uint legacy = _regs.ReadCapability(capability);
        if ((legacy & LegacyBiosOwned) != 0)
        {
            _regs.WriteCapability(capability, legacy | LegacyOsOwned);
            uint waitedMs = 0;
            while ((_regs.ReadCapability(capability) & LegacyBiosOwned) != 0 && waitedMs < FirmwareHandoffTimeoutMs)
            {
                UsbManager.DelayMilliseconds(1);
                waitedMs++;
            }

            if ((_regs.ReadCapability(capability) & LegacyBiosOwned) != 0)
            {
                Serial.WriteString("[xHCI] Firmware did not release the controller, taking it over\n");
                _regs.WriteCapability(capability, (_regs.ReadCapability(capability) & ~LegacyBiosOwned) | LegacyOsOwned);
            }
        }

        ulong controlStatus = capability + LegacyControlStatusOffset;
        _regs.WriteCapability(controlStatus, (_regs.ReadCapability(controlStatus) & LegacyDisableSmiMask) | LegacySmiEvents);
    }

    /// <summary>
    /// Byte offset of the next extended capability with <paramref name="id"/>
    /// after the one at <paramref name="after"/> (0 to start from the
    /// first), or 0 when there is none.
    /// </summary>
    private ulong FindExtendedCapability(byte id, ulong after)
    {
        ulong capability = after == 0 ? _regs.ExtendedCapabilitiesAddress : NextExtendedCapability(after);
        while (capability != 0)
        {
            if ((_regs.ReadCapability(capability) & ExtendedCapabilityIdMask) == id)
            {
                return capability;
            }

            capability = NextExtendedCapability(capability);
        }

        return 0;
    }

    private ulong NextExtendedCapability(ulong capability)
    {
        uint next = (_regs.ReadCapability(capability) >> ExtendedCapabilityNextShift) & ExtendedCapabilityNextMask;
        return next == 0 ? 0 : capability + ((ulong)next << DwordShift);
    }

    /// <summary>Stops the controller and resets it to its power-on state (xHCI 1.2 §4.2).</summary>
    private void Reset()
    {
        if (!WaitForStatus(UsbStsControllerNotReady, 0, ResetTimeoutMs))
        {
            throw new InvalidOperationException("[xHCI] Controller stayed not ready");
        }

        _regs.UsbCmd &= ~(UsbCmdRun | UsbCmdInterrupterEnable);
        if (!WaitForStatus(UsbStsHalted, UsbStsHalted, HaltTimeoutMs))
        {
            throw new InvalidOperationException("[xHCI] Controller did not halt");
        }

        _regs.UsbCmd |= UsbCmdReset;
        UsbManager.DelayMilliseconds(ResetSettleMs);
        uint waitedMs = 0;
        while ((_regs.UsbCmd & UsbCmdReset) != 0)
        {
            if (waitedMs++ >= ResetTimeoutMs)
            {
                throw new InvalidOperationException("[xHCI] Controller reset did not complete");
            }

            UsbManager.DelayMilliseconds(1);
        }

        if (!WaitForStatus(UsbStsControllerNotReady, 0, ResetTimeoutMs))
        {
            throw new InvalidOperationException("[xHCI] Controller not ready after reset");
        }
    }

    /// <summary>
    /// Gives the controller the private pages it asked for in HCSPARAMS2,
    /// through entry 0 of the device context array (xHCI 1.2 §4.20).
    /// </summary>
    private void SetupScratchpad()
    {
        int count = _regs.MaxScratchpadBuffers;
        if (count == 0)
        {
            return;
        }

        ulong arrayPages = (((ulong)count * sizeof(ulong)) + (ulong)XhciDma.PageSize - 1) / (ulong)XhciDma.PageSize;
        ulong* array = (ulong*)XhciDma.AllocPages(arrayPages, out ulong arrayAddress);
        for (int i = 0; i < count; i++)
        {
            XhciDma.AllocPages(1, out ulong bufferAddress);
            array[i] = bufferAddress;
        }

        _deviceContextArray[0] = arrayAddress;
    }

    /// <summary>
    /// Routes interrupter 0 to an MSI-X vector. Without MSI-X (no capability,
    /// or no platform MSI backend) events are only processed by the
    /// synchronous waits and <see cref="Poll"/>.
    /// </summary>
    private void SetupInterrupts()
    {
        MsiXContext? context = MsiX.Enable(_pci);
        if (context is null)
        {
            return;
        }

        MsiX.SetEntry(context.Value, 0, OnInterrupt);
        _regs.Iman = ImanInterruptEnable | ImanInterruptPending;
        _msiXEnabled = true;
    }

    private bool WaitForStatus(uint mask, uint expected, uint timeoutMs)
    {
        for (uint waitedMs = 0; ; waitedMs++)
        {
            if ((_regs.UsbSts & mask) == expected)
            {
                return true;
            }

            if (waitedMs >= timeoutMs)
            {
                return false;
            }

            UsbManager.DelayMilliseconds(1);
        }
    }
}
