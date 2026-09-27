// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Registers;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

/// <summary>
/// xHCI host controller driver (eXtensible Host Controller Interface 1.2),
/// the controller behind every USB port of a PC since ~2012 and of QEMU's
/// qemu-xhci. Device-class agnostic: it addresses devices, runs control
/// and bulk transfers, opens interrupt endpoints and registers hubs, and
/// leaves what the devices are to the kit's USB core and its class drivers.
///
/// <para>Commands, control transfers and bulk transfers are synchronous
/// and only issued from thread context. Completions arrive on one event
/// ring, drained by the interrupt handler, by the synchronous waits
/// themselves, and by <see cref="Poll"/> when the kit could grant no
/// interrupts.</para>
///
/// <para>Two locks, always taken in this order: <c>_eventLock</c> covers
/// the event ring and the state of the synchronous waits;
/// <c>_ringLock</c> covers the command ring and every transfer ring.
/// Report handlers run under the event lock only, so they may queue
/// transfers (keyboard LEDs) without deadlocking. Two turns, taken before
/// either lock, serialize the waits: one for commands, one for control
/// transfers, which may issue commands of their own.</para>
///
/// <para>Root port changes raise a Port Status Change Event, which only
/// wakes the USB core's hot-plug thread (after making the transfers of a
/// device that left fail at once); the ports themselves are handled on
/// that thread, in <see cref="HandlePortChanges"/>.</para>
///
/// <para>Not implemented yet: isochronous endpoints, interrupt OUT
/// endpoints and streams.</para>
/// </summary>
internal sealed partial class XhciController : UsbHostController
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

    /// <summary>Faults the event handler keeps for the hot-plug thread to log; more are counted, not kept.</summary>
    private const int MaxPendingFaults = 8;

    private readonly PciDeviceContext _context;
    private readonly ControllerRegisters _registers;
    private readonly XhciMemory _memory;
    private readonly ProducerRing _commandRing;
    private readonly EventRing _eventRing;

    /// <summary>The Device Context Base Address Array: slot N's output context address at entry N, the scratchpad array at entry 0.</summary>
    private readonly DmaBuffer _deviceContextArray;

    /// <summary>Addressed devices by slot ID (index 0 is unused: slot IDs start at 1).</summary>
    private readonly XhciDevice?[] _devices;

    /// <summary>USB major revision of each root port (index port - 1), from the Supported Protocol capabilities.</summary>
    private readonly byte[] _portMajorRevision;

    private readonly IrqSafeLock _eventLock = new();
    private readonly IrqSafeLock _ringLock = new();

    /// <summary>What the event handler recorded for the hot-plug thread to log, under the event lock.</summary>
    private readonly XhciFault[] _faults = new XhciFault[MaxPendingFaults];

    private int _faultCount;
    private int _faultsDropped;

    /// <summary>
    /// The bus the USB core gave the controller when it probed its root
    /// ports, which the event handler reports port changes through; null
    /// before that.
    /// </summary>
    private volatile UsbBus? _bus;

    /// <summary>Whether the kit calls <see cref="OnInterrupt"/>, through MSI-X or from the timer.</summary>
    private bool _interruptsDelivered;

    public override string Name => "xHCI";

    public override bool IsPolled => !_interruptsDelivered;

    private XhciController(PciDeviceContext context, ControllerRegisters registers, XhciMemory memory)
    {
        _context = context;
        _registers = registers;
        _memory = memory;
        _devices = new XhciDevice?[registers.MaxSlots + 1];
        _portMajorRevision = new byte[registers.MaxPorts];
        _commandRing = new ProducerRing(memory);
        _eventRing = new EventRing(memory);
        _deviceContextArray = memory.Allocate(1);

        // Each holds one signal while free, so a wait on it takes the turn.
        _commandTurn = context.CreateEvent();
        _commandTurn.Signal();
        _controlTurn = context.CreateEvent();
        _controlTurn.Signal();
    }

    /// <summary>
    /// Reads the controller's register block and allocates what it runs on:
    /// the command and event rings and the device context array. Probe only.
    /// </summary>
    /// <param name="context">The binding.</param>
    /// <param name="registers">BAR0, as the kit mapped it.</param>
    /// <exception cref="InvalidOperationException">BAR0 is too small for the registers the controller declares, or no DMA memory the controller can reach is free.</exception>
    internal static XhciController Create(PciDeviceContext context, MmioRegion registers)
    {
        ControllerRegisters controllerRegisters = new(registers);
        if (controllerRegisters.RequiredLength > registers.Length)
        {
            throw new InvalidOperationException(
                $"BAR 0 holds {registers.Length} bytes, fewer than the {controllerRegisters.RequiredLength} its registers span.");
        }

        return new XhciController(context, controllerRegisters, new XhciMemory(context, controllerRegisters.Is64BitCapable));
    }

    /// <summary>
    /// Takes the controller from firmware, resets it and starts it, with its
    /// interrupts requested from the kit. The root ports are left for the
    /// USB core to probe once the kit delivered the controller. Probe only.
    /// </summary>
    /// <exception cref="InvalidOperationException">The controller cannot be brought up.</exception>
    internal void Start()
    {
        WriteLog($"version 0x{_registers.HciVersion:X4}, {_registers.MaxSlots} slots, {_registers.MaxPorts} ports, "
            + $"{_registers.ContextSize}-byte contexts, {_registers.MaxScratchpadBuffers} scratchpad buffers");

        if ((_registers.PageSize & PageSize4KiB) == 0)
        {
            throw new InvalidOperationException("The controller does not support 4 KiB pages.");
        }

        if (!_registers.Is64BitCapable)
        {
            WriteLog("the controller takes 32-bit DMA addresses only");
        }

        TakeOwnershipFromFirmware();
        ReadSupportedProtocols();
        Reset();

        // After the reset, which left no DMA state firmware set up, and
        // before the rings are programmed: the controller reads the Event
        // Ring Segment Table as soon as ERSTBA is written (xHCI 1.2 §4.9.4),
        // and loses its event ring if it cannot.
        _context.EnableBusMastering();

        _registers.Config = _registers.MaxSlots;
        SetupScratchpad();
        _registers.Dcbaap = _deviceContextArray.DeviceAddress;
        _registers.Crcr = _commandRing.DeviceAddress | CrcrRingCycleState;

        // ERSTBA is written last: it is what arms the event ring (xHCI 1.2 §4.9.4).
        _registers.Imod = InterruptModerationInterval;
        _registers.Erstsz = EventRing.SegmentCount;
        _registers.Erdp = _eventRing.DequeuePointer;
        _registers.Erstba = _eventRing.SegmentTableAddress;

        // Through MSI-X, or polled from the timer; either way the handler
        // only runs once the probe returned Bound and the USB core probed
        // the root ports, whose waits drain the events themselves.
        _interruptsDelivered = _context.TryRequestInterrupts(OnInterrupt);
        if (_interruptsDelivered)
        {
            _registers.Iman = ImanInterruptEnable | ImanInterruptPending;
        }

        _registers.UsbCmd |= UsbCmdRun | (_interruptsDelivered ? UsbCmdInterrupterEnable : 0);
        if (!WaitForStatus(UsbStsHalted, 0, HaltTimeoutMs))
        {
            throw new InvalidOperationException("The controller did not start.");
        }

        WriteLog(_interruptsDelivered ? "running" : "running, events polled by the USB hot-plug thread");
    }

    /// <summary>
    /// Stops a controller whose probe failed, best effort, so it no longer
    /// fetches from the rings the kit is about to free. Probe only.
    /// </summary>
    internal void Halt()
    {
        _registers.UsbCmd &= ~(UsbCmdRun | UsbCmdInterrupterEnable);
        if (!WaitForStatus(UsbStsHalted, UsbStsHalted, HaltTimeoutMs))
        {
            WriteLog("the controller did not halt after the failed probe");
        }
    }

    /// <inheritdoc />
    protected override void Poll()
    {
        using (_eventLock.EnterScope())
        {
            DrainEvents();
        }

        WriteFaults();
    }

    /// <inheritdoc />
    protected override void ReleaseDevice(UsbHostDevice device)
    {
        if (device is not XhciDevice xhciDevice || xhciDevice.Controller != this)
        {
            return;
        }

        // A transfer still waiting on the device gives up once it sees it
        // disconnected: taking the control turn once, and waiting until no
        // bulk transfer is counted in, guarantees none is left using what is
        // freed below.
        xhciDevice.MarkGone();
        if (_controlTurn.Wait())
        {
            _controlTurn.Signal();
        }

        while (xhciDevice.HasBulkTransfers)
        {
            DelayMilliseconds(1);
        }

        byte slotId = xhciDevice.SlotId;
        CompletionCode code = ExecuteCommand(0, SlotCommand(TrbType.DisableSlotCommand, slotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Disable Slot", code);
        }

        // Past the event lock no event reaches the device's pipes, past the
        // ring lock no fire-and-forget transfer is mid-enqueue on its rings.
        using (_eventLock.EnterScope())
        {
            using (_ringLock.EnterScope())
            {
                _devices[slotId] = null;
                WriteDeviceContextPointer(slotId, 0);
            }
        }

        xhciDevice.Free(_memory);
    }

    /// <summary>Points entry <paramref name="slotId"/> of the device context array at an output context, or at nothing.</summary>
    private void WriteDeviceContextPointer(byte slotId, ulong address) =>
        BinaryPrimitives.WriteUInt64LittleEndian(_deviceContextArray.Span[(slotId * sizeof(ulong))..], address);

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

        uint legacy = _registers.ReadCapability(capability);
        if ((legacy & LegacyBiosOwned) != 0)
        {
            _registers.WriteCapability(capability, legacy | LegacyOsOwned);
            uint waitedMs = 0;
            while ((_registers.ReadCapability(capability) & LegacyBiosOwned) != 0 && waitedMs < FirmwareHandoffTimeoutMs)
            {
                DelayMilliseconds(1);
                waitedMs++;
            }

            if ((_registers.ReadCapability(capability) & LegacyBiosOwned) != 0)
            {
                WriteLog("firmware did not release the controller, taking it over");
                _registers.WriteCapability(capability, (_registers.ReadCapability(capability) & ~LegacyBiosOwned) | LegacyOsOwned);
            }
        }

        ulong controlStatus = capability + LegacyControlStatusOffset;
        _registers.WriteCapability(controlStatus, (_registers.ReadCapability(controlStatus) & LegacyDisableSmiMask) | LegacySmiEvents);
    }

    /// <summary>
    /// Byte offset of the next extended capability with <paramref name="id"/>
    /// after the one at <paramref name="after"/> (0 to start from the
    /// first), or 0 when there is none.
    /// </summary>
    private ulong FindExtendedCapability(byte id, ulong after)
    {
        ulong capability = after == 0 ? _registers.ExtendedCapabilitiesAddress : NextExtendedCapability(after);
        while (capability != 0)
        {
            if ((_registers.ReadCapability(capability) & ExtendedCapabilityIdMask) == id)
            {
                return capability;
            }

            capability = NextExtendedCapability(capability);
        }

        return 0;
    }

    private ulong NextExtendedCapability(ulong capability)
    {
        uint next = (_registers.ReadCapability(capability) >> ExtendedCapabilityNextShift) & ExtendedCapabilityNextMask;
        return next == 0 ? 0 : capability + ((ulong)next << DwordShift);
    }

    /// <summary>Stops the controller and resets it to its power-on state (xHCI 1.2 §4.2).</summary>
    private void Reset()
    {
        if (!WaitForStatus(UsbStsControllerNotReady, 0, ResetTimeoutMs))
        {
            throw new InvalidOperationException("The controller stayed not ready.");
        }

        _registers.UsbCmd &= ~(UsbCmdRun | UsbCmdInterrupterEnable);
        if (!WaitForStatus(UsbStsHalted, UsbStsHalted, HaltTimeoutMs))
        {
            throw new InvalidOperationException("The controller did not halt.");
        }

        _registers.UsbCmd |= UsbCmdReset;
        DelayMilliseconds(ResetSettleMs);
        uint waitedMs = 0;
        while ((_registers.UsbCmd & UsbCmdReset) != 0)
        {
            if (waitedMs++ >= ResetTimeoutMs)
            {
                throw new InvalidOperationException("The controller reset did not complete.");
            }

            DelayMilliseconds(1);
        }

        if (!WaitForStatus(UsbStsControllerNotReady, 0, ResetTimeoutMs))
        {
            throw new InvalidOperationException("The controller was not ready after the reset.");
        }
    }

    /// <summary>
    /// Gives the controller the private pages it asked for in HCSPARAMS2,
    /// through entry 0 of the device context array (xHCI 1.2 §4.20). They
    /// stay the controller's for as long as it runs.
    /// </summary>
    private void SetupScratchpad()
    {
        int count = _registers.MaxScratchpadBuffers;
        if (count == 0)
        {
            return;
        }

        int arrayPages = ((count * sizeof(ulong)) + XhciMemory.PageSize - 1) / XhciMemory.PageSize;
        DmaBuffer array = _memory.Allocate(arrayPages);
        Span<byte> entries = array.Span;
        for (int i = 0; i < count; i++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(entries[(i * sizeof(ulong))..], _memory.Allocate(1).DeviceAddress);
        }

        WriteDeviceContextPointer(0, array.DeviceAddress);
    }

    private bool WaitForStatus(uint mask, uint expected, uint timeoutMs)
    {
        for (uint waitedMs = 0; ; waitedMs++)
        {
            if ((_registers.UsbSts & mask) == expected)
            {
                return true;
            }

            if (waitedMs >= timeoutMs)
            {
                return false;
            }

            DelayMilliseconds(1);
        }
    }

    /// <summary>
    /// Busy-waits: the probe and the first root port probe run on the boot
    /// thread, which is the idle thread and cannot sleep. Thread context.
    /// </summary>
    private void DelayMilliseconds(uint milliseconds) => _context.Delay(TimeSpan.FromMilliseconds(milliseconds));

    /// <summary>Writes one line to the log, under the binding's prefix. Thread context.</summary>
    private void WriteLog(string message) => _context.WriteLog(message);
}
