// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="XhciDriver"/> holds for one bound controller, hung
/// off <see cref="DeviceBinding.DriverState"/>, and the kit's
/// <see cref="UsbHostController"/> for it: the register window and its
/// four register sets, the command ring, the event ring, the device context
/// base address array, the scratchpad, the slot table, the pooled slot and
/// pipe memory sets, the kit lock, the interrupt outcome and the counters
/// the suites read. Commands, control transfers and bulk transfers are
/// synchronous and run in thread context; completions arrive on one event
/// ring, drained by the message interrupt handler (no lock: a holder of the
/// <see cref="DeviceLock"/> runs with interrupts disabled, so the two never
/// overlap) or, on a polled controller and while the host binding detaches,
/// by the waiting thread and the hot-plug thread under the lock. The lock
/// guards the command ring, every transfer ring, the event ring consumer,
/// the slot table and the pools, and is never held across a wait, a delay,
/// a sleep or a kit call other than a report delivery on a polled
/// controller. The three busy flags (<see cref="_commandBusy"/>, a slot's
/// <see cref="XhciSlot.ControlBusy"/>, a bulk pipe's
/// <see cref="XhciBulkPipe.Busy"/>) are claimed under the lock in a delay
/// loop bounded by the operation's own timeout. The partials hold the
/// commands, the control transfers, the bulk transfers, the events and the
/// ports.
/// </summary>
public sealed partial class XhciState : UsbHostController
{
    // --- Constants ---

    /// <summary>Bytes of one page: every ring, context and buffer page is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>Bytes of one entry of the device context base address array and of the scratchpad array.</summary>
    private const int PointerBytes = 8;

    /// <summary>Entry 0 of the device context base address array points at the scratchpad array.</summary>
    private const int ScratchpadEntry = 0;

    /// <summary>Bits in the low dword of a 64-bit register.</summary>
    private const int UpperDwordShift = 32;

    /// <summary>The two context sizes HCCPARAMS1.CSZ selects between.</summary>
    private const int ContextSize32 = 32;
    private const int ContextSize64 = 64;

    /// <summary>The probe's failure text when a 32-bit controller finds no memory below 4 GiB.</summary>
    private const string NoLowMemoryFailure = "no DMA memory below 4 GiB for a 32-bit controller";

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly ulong _operationalBase;
    private readonly ulong _runtimeBase;
    private readonly ulong _doorbellBase;
    private readonly ulong _extendedCapabilities;
    private readonly ushort _version;
    private readonly byte _maxSlots;
    private readonly byte _maxPorts;
    private readonly int _contextSize;
    private readonly int _scratchpadBuffers;
    private readonly bool _is64BitCapable;
    private readonly bool _hasPortPowerControl;
    private readonly byte[] _portMajorRevision;
    private readonly XhciSlot?[] _slots;
    private readonly List<XhciSlotMemory> _freeSlotSets;
    private readonly List<XhciPipeMemory> _freeInterruptPipeMemory = [];
    private readonly List<XhciPipeMemory> _freeBulkPipeMemory = [];
    private readonly DeviceEvent _commandEvent;
    private readonly DeviceEvent _portChangeEvent;
    private readonly WorkItem _faultReport;
    private XhciRing? _commandRing;
    private XhciEventRing? _eventRing;
    private DmaBuffer? _dcbaa;
    private DmaBuffer? _scratchpadArray;
    private DeviceLock? _lock;
    private UsbBus? _bus;
    private volatile bool _running;
    private volatile bool _rootPortsProbed;

    // Synchronous command state: one command in flight per controller,
    // claimed through _commandBusy under the lock; the completion fields are
    // written by the handler or the polled drain before _commandCompleted.
    private bool _commandBusy;
    private ulong _pendingCommand;
    private bool _commandCompleted;
    private XhciCompletionCode _commandCode;
    private byte _commandSlotId;

    // What the handler saw and could not log: the fault report work item
    // logs it in thread context.
    private volatile bool _transferFaultPending;
    private byte _transferFaultSlot;
    private byte _transferFaultEndpoint;
    private XhciCompletionCode _transferFaultCode;
    private volatile bool _hostEventPending;
    private XhciCompletionCode _hostEventCode;

    private int _index;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile bool _hotPlugRunning;
    private volatile int _commandsIssued;
    private volatile int _timeouts;
    private volatile int _devicesAddressed;
    private volatile int _pipesClosed;
    private volatile int _interruptCount;
    private volatile int _pipeRecoveries;
    private volatile int _hostControllerEvents;
    private volatile byte _lastHostControllerEventCode;

    // --- Constructor ---

    /// <summary>
    /// Reads the capability registers (CAPLENGTH, HCIVERSION, HCSPARAMS1,
    /// HCSPARAMS2, HCCPARAMS1, DBOFF, RTSOFF), locates the operational,
    /// runtime and doorbell register sets, sizes the slot table and creates
    /// the command and port change events and the fault report work item.
    /// The rings come through <see cref="AllocateStructures"/> and the lock
    /// through <see cref="Lock"/>. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The function's binding, for the DMA memory, the events, the delays, the waits and the log.</param>
    /// <param name="registers">The BAR0 window.</param>
    internal XhciState(DeviceBinding binding, RegisterWindow registers)
    {
        _binding = binding;
        _registers = registers;

        uint capabilityDword = registers.Read32(XhciProtocol.CapLength);
        _operationalBase = registers.Read8(XhciProtocol.CapLength);
        _version = (ushort)(capabilityDword >> XhciProtocol.HciVersionShift);
        _runtimeBase = registers.Read32(XhciProtocol.RuntimeOffset) & XhciProtocol.RuntimeOffsetMask;
        _doorbellBase = registers.Read32(XhciProtocol.DoorbellOffset) & XhciProtocol.DoorbellOffsetMask;

        uint hcsParams1 = registers.Read32(XhciProtocol.HcsParams1);
        _maxSlots = (byte)(hcsParams1 & XhciProtocol.HcsParams1MaxSlotsMask);
        _maxPorts = (byte)((hcsParams1 >> XhciProtocol.HcsParams1MaxPortsShift) & XhciProtocol.HcsParams1MaxPortsMask);

        uint hcsParams2 = registers.Read32(XhciProtocol.HcsParams2);
        uint scratchpadHigh = (hcsParams2 >> XhciProtocol.ScratchpadHighShift) & XhciProtocol.ScratchpadPartMask;
        uint scratchpadLow = (hcsParams2 >> XhciProtocol.ScratchpadLowShift) & XhciProtocol.ScratchpadPartMask;
        _scratchpadBuffers = (int)((scratchpadHigh << XhciProtocol.ScratchpadHighPartBits) | scratchpadLow);

        uint hccParams1 = registers.Read32(XhciProtocol.HccParams1);
        _is64BitCapable = (hccParams1 & XhciProtocol.HccParams1Ac64) != 0;
        _contextSize = (hccParams1 & XhciProtocol.HccParams1ContextSize64) != 0 ? ContextSize64 : ContextSize32;
        _hasPortPowerControl = (hccParams1 & XhciProtocol.HccParams1PortPowerControl) != 0;
        _extendedCapabilities = ((hccParams1 >> XhciProtocol.HccParams1ExtendedCapabilitiesShift) & XhciProtocol.HccParams1ExtendedCapabilitiesMask) << XhciProtocol.DwordShift;

        _portMajorRevision = new byte[_maxPorts];
        _slots = new XhciSlot?[_maxSlots + 1];
        _freeSlotSets = new List<XhciSlotMemory>(_maxSlots + 1);
        _commandEvent = binding.CreateEvent();
        _portChangeEvent = binding.CreateEvent();
        _faultReport = binding.CreateWorkItem(ReportFaults);
    }

    // --- Properties the suites read ---

    /// <inheritdoc/>
    public override string Name => "xHCI";

    /// <summary>The controller's number, 0 for the first one bound; assigned when the probe binds. Any context.</summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>HCIVERSION, 0x100 for a 1.0.0 controller. Any context.</summary>
    public ushort Version => _version;

    /// <summary>HCSPARAMS1.MaxSlots: device slots the controller supports. Any context.</summary>
    public byte MaxSlots => _maxSlots;

    /// <summary>HCSPARAMS1.MaxPorts: root hub ports. Any context.</summary>
    public byte MaxPorts => _maxPorts;

    /// <summary>Bytes of one Slot, Endpoint or Input Control context, 32 or 64 (HCCPARAMS1.CSZ). Any context.</summary>
    public int ContextSize => _contextSize;

    /// <summary>HCSPARAMS2.MaxScratchpadBuffers: private pages the controller asked for. Any context.</summary>
    public int ScratchpadBuffers => _scratchpadBuffers;

    /// <summary>True when a message interrupt is connected to <see cref="OnInterrupt"/>, so a wait parks on an event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when no message interrupt could be connected, so the waiting thread and the hot-plug thread drain the event ring. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True once the <c>xhci-hotplug</c> thread started; false without a scheduler, when only boot-time devices are enumerated. Any context.</summary>
    public bool HotPlugRunning
    {
        get => _hotPlugRunning;
        internal set => _hotPlugRunning = value;
    }

    /// <summary>The bus the probe created over this controller, with the device tree below it. Any context once the probe bound.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the bus.</exception>
    public UsbBus Bus
    {
        get => _bus ?? throw new InvalidOperationException("The controller's bus is not created.");
        internal set => _bus = value;
    }

    /// <summary>How many commands were put on the command ring by <see cref="ExecuteCommand"/>. Any context.</summary>
    public int CommandsIssued => _commandsIssued;

    /// <summary>How many of those went unanswered for <see cref="XhciProtocol.CommandTimeoutMs"/>. Any context.</summary>
    public int Timeouts => _timeouts;

    /// <summary>How many devices Address Device gave an address and a descriptor. Any context.</summary>
    public int DevicesAddressed => _devicesAddressed;

    /// <summary>How many pipes were closed, by a driver or by a binding's unwind. Any context.</summary>
    public int PipesClosed => _pipesClosed;

    /// <summary>How many times the message interrupt ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many interrupt pipe recoveries were started after a transfer error. Any context.</summary>
    public int PipeRecoveries => _pipeRecoveries;

    /// <summary>How many Host Controller Events the controller raised. Any context.</summary>
    public int HostControllerEvents => _hostControllerEvents;

    /// <summary>The completion code of the last Host Controller Event, 0 when none came. Any context.</summary>
    public byte LastHostControllerEventCode => _lastHostControllerEventCode;

    // --- Internal properties the probe reads and sets ---

    /// <summary>The lock every ring write, event dequeue and table change runs under; set by the probe before the handler is connected.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the lock.</exception>
    internal DeviceLock Lock
    {
        get => _lock ?? throw new InvalidOperationException("The controller's lock is not created.");
        set => _lock = value;
    }

    /// <summary>HCCPARAMS1.AC64: the controller takes 64-bit DMA addresses; without it every allocation is held below 4 GiB.</summary>
    internal bool Is64BitCapable => _is64BitCapable;

    /// <summary>HCCPARAMS1.PPC: software switches port power.</summary>
    internal bool HasPortPowerControl => _hasPortPowerControl;

    /// <summary>Bytes from BAR0 the driver touches: the last port register set, the first interrupter and the last doorbell, whichever lies furthest.</summary>
    internal ulong RequiredRegisterLength
    {
        get
        {
            ulong ports = _operationalBase + XhciProtocol.PortRegisterSetOffset + (XhciProtocol.PortRegisterSetStride * _maxPorts);
            ulong runtime = _runtimeBase + XhciProtocol.Interrupter0Offset + XhciProtocol.InterrupterSetSize;
            ulong doorbells = _doorbellBase + (XhciProtocol.DoorbellStride * ((ulong)_maxSlots + 1));
            return Math.Max(ports, Math.Max(runtime, doorbells));
        }
    }

    /// <summary>PAGESIZE bit 0: the controller supports 4 KiB pages, the only size the driver uses.</summary>
    internal bool Supports4KiBPages => (ReadOperational(XhciProtocol.PageSize) & XhciProtocol.PageSize4KiB) != 0;

    /// <summary>True from the controller's start to its stop: the window in which endpoint commands are worth issuing.</summary>
    internal bool Running
    {
        get => _running;
        set => _running = value;
    }

    /// <summary>
    /// <see cref="Running"/> and USBSTS.HCH clear: a halted controller runs
    /// no command, and a PCI function that vanished reads all ones, HCH
    /// included, so a pipe close or an endpoint stop against either skips
    /// its commands instead of waiting out their budgets. Any context.
    /// </summary>
    private bool ControllerRuns => _running && (ReadOperational(XhciProtocol.UsbSts) & XhciProtocol.UsbStsHalted) == 0;

    /// <summary>The command ring, once allocated.</summary>
    private XhciRing CommandRing => _commandRing ?? throw new InvalidOperationException("The command ring is not allocated.");

    /// <summary>The device context base address array, once allocated.</summary>
    private DmaBuffer Dcbaa => _dcbaa ?? throw new InvalidOperationException("The device context base address array is not allocated.");

    // --- Internal methods for the probe and the detach hook ---

    /// <summary>
    /// Allocates the command ring, the event ring (its segment and its
    /// segment table), the device context base address array and, when the
    /// controller asked for scratchpad buffers, the scratchpad array and its
    /// pages, each pointer written into the array and the array into entry
    /// 0 (xHCI 1.2 section 4.20). Thread context, from the probe.
    /// </summary>
    /// <returns>Null when everything was allocated; the probe's failure text when a 32-bit controller found no memory below 4 GiB.</returns>
    internal string? AllocateStructures()
    {
        DmaBuffer? commandRing = AllocateDma(PageBytes, PageBytes);
        DmaBuffer? eventSegment = AllocateDma(PageBytes, PageBytes);
        DmaBuffer? eventTable = AllocateDma(PageBytes, PageBytes);
        DmaBuffer? dcbaa = AllocateDma(PageBytes, PageBytes);
        if (commandRing is null || eventSegment is null || eventTable is null || dcbaa is null)
        {
            return NoLowMemoryFailure;
        }

        _commandRing = new XhciRing(commandRing);
        _eventRing = new XhciEventRing(eventSegment, eventTable);
        _dcbaa = dcbaa;

        if (_scratchpadBuffers > 0)
        {
            int arrayPages = ((_scratchpadBuffers * PointerBytes) + PageBytes - 1) / PageBytes;
            DmaBuffer? array = AllocateDma(arrayPages * PageBytes, PageBytes);
            if (array is null)
            {
                return NoLowMemoryFailure;
            }

            Span<ulong> pointers = MemoryMarshal.Cast<byte, ulong>(array.Span);
            for (int i = 0; i < _scratchpadBuffers; i++)
            {
                DmaBuffer? page = AllocateDma(PageBytes, PageBytes);
                if (page is null)
                {
                    return NoLowMemoryFailure;
                }

                pointers[i] = page.PhysicalAddress;
            }

            Span<ulong> entries = MemoryMarshal.Cast<byte, ulong>(dcbaa.Span);
            entries[ScratchpadEntry] = array.PhysicalAddress;
            _scratchpadArray = array;
            DmaBuffer.WriteBarrier();
        }

        return null;
    }

    /// <summary>
    /// Asks the firmware to hand the controller over through the USB Legacy
    /// Support capability (set OS Owned, poll BIOS Owned clear for up to
    /// <see cref="XhciProtocol.FirmwareHandoffTimeoutMs"/>, else log and
    /// clear it), then turns off the SMIs it may still raise for PS/2
    /// emulation (xHCI 1.2 section 4.22.1). Thread context, from the probe.
    /// </summary>
    internal void TakeOwnershipFromFirmware()
    {
        ulong capability = FindExtendedCapability(XhciProtocol.LegacySupportCapabilityId, 0);
        if (capability == 0)
        {
            return;
        }

        uint legacy = _registers.Read32(capability);
        if ((legacy & XhciProtocol.LegacyBiosOwned) != 0)
        {
            _registers.Write32(capability, legacy | XhciProtocol.LegacyOsOwned);
            uint waitedMs = 0;
            while ((_registers.Read32(capability) & XhciProtocol.LegacyBiosOwned) != 0 && waitedMs < XhciProtocol.FirmwareHandoffTimeoutMs)
            {
                _binding.Delay(XhciProtocol.MicrosecondsPerMillisecond);
                waitedMs++;
            }

            if ((_registers.Read32(capability) & XhciProtocol.LegacyBiosOwned) != 0)
            {
                _binding.Log("firmware did not release the controller, taking it over");
                _registers.Write32(capability, (_registers.Read32(capability) & ~XhciProtocol.LegacyBiosOwned) | XhciProtocol.LegacyOsOwned);
            }
        }

        ulong controlStatus = capability + XhciProtocol.LegacyControlStatusOffset;
        _registers.Write32(controlStatus, (_registers.Read32(controlStatus) & XhciProtocol.LegacyDisableSmiMask) | XhciProtocol.LegacySmiEvents);
    }

    /// <summary>
    /// Stops the controller and resets it to its power-on state (xHCI 1.2
    /// section 4.2): CNR clear, Run and INTE cleared and HCH awaited, HCRST
    /// set, a settle, HCRST clear awaited, CNR clear again. Thread context,
    /// from the probe.
    /// </summary>
    /// <returns>Null when the reset completed; the probe's failure text otherwise.</returns>
    internal string? Reset()
    {
        if (!WaitForStatus(XhciProtocol.UsbStsControllerNotReady, 0, XhciProtocol.ResetTimeoutMs))
        {
            return "the controller stayed not ready";
        }

        WriteOperational(XhciProtocol.UsbCmd, ReadOperational(XhciProtocol.UsbCmd) & ~(XhciProtocol.UsbCmdRun | XhciProtocol.UsbCmdInterrupterEnable));
        if (!WaitForStatus(XhciProtocol.UsbStsHalted, XhciProtocol.UsbStsHalted, XhciProtocol.HaltTimeoutMs))
        {
            return "the controller did not halt";
        }

        WriteOperational(XhciProtocol.UsbCmd, ReadOperational(XhciProtocol.UsbCmd) | XhciProtocol.UsbCmdReset);
        _binding.Delay(XhciProtocol.ResetSettleMs * XhciProtocol.MicrosecondsPerMillisecond);
        uint waitedMs = 0;
        while ((ReadOperational(XhciProtocol.UsbCmd) & XhciProtocol.UsbCmdReset) != 0)
        {
            if (waitedMs++ >= XhciProtocol.ResetTimeoutMs)
            {
                return "the controller reset did not complete";
            }

            _binding.Delay(XhciProtocol.MicrosecondsPerMillisecond);
        }

        if (!WaitForStatus(XhciProtocol.UsbStsControllerNotReady, 0, XhciProtocol.ResetTimeoutMs))
        {
            return "the controller is not ready after reset";
        }

        return null;
    }

    /// <summary>
    /// Programs the reset controller: CONFIG with MaxSlots, DCBAAP, CRCR
    /// with the command ring and RCS, IMOD, ERSTSZ, ERDP and ERSTBA last,
    /// since ERSTBA is what arms the event ring (xHCI 1.2 section 4.9.4).
    /// The 64-bit registers are written low dword first. Thread context,
    /// from the probe.
    /// </summary>
    internal void ProgramRegisters()
    {
        XhciEventRing eventRing = _eventRing ?? throw new InvalidOperationException("The event ring is not allocated.");
        WriteOperational(XhciProtocol.Config, _maxSlots);
        WriteOperational64(XhciProtocol.Dcbaap, Dcbaa.PhysicalAddress);
        WriteOperational64(XhciProtocol.Crcr, CommandRing.PhysicalAddress | XhciProtocol.CrcrRingCycleState);
        WriteInterrupter(XhciProtocol.Imod, XhciProtocol.InterruptModerationInterval);
        WriteInterrupter(XhciProtocol.Erstsz, XhciEventRing.SegmentCount);
        WriteInterrupter64(XhciProtocol.Erdp, eventRing.DequeuePointer);
        WriteInterrupter64(XhciProtocol.Erstba, eventRing.SegmentTableAddress);
    }

    /// <summary>Enables interrupter 0 (IMAN.IE, IP written back as set), once the message interrupt is connected. Thread context, from the probe.</summary>
    internal void EnableInterrupter() =>
        WriteInterrupter(XhciProtocol.Iman, XhciProtocol.ImanInterruptEnable | XhciProtocol.ImanInterruptPending);

    /// <summary>Sets USBCMD.Run (and INTE with an interrupt) and waits for HCH to clear within <see cref="XhciProtocol.HaltTimeoutMs"/>. Thread context, from the probe.</summary>
    /// <param name="interrupts">Whether a message interrupt is connected.</param>
    /// <returns>False when the controller did not start.</returns>
    internal bool Start(bool interrupts)
    {
        uint command = ReadOperational(XhciProtocol.UsbCmd) | XhciProtocol.UsbCmdRun | (interrupts ? XhciProtocol.UsbCmdInterrupterEnable : 0);
        WriteOperational(XhciProtocol.UsbCmd, command);
        if (!WaitForStatus(XhciProtocol.UsbStsHalted, 0, XhciProtocol.HaltTimeoutMs))
        {
            return false;
        }

        _running = true;
        return true;
    }

    /// <summary>Clears USBCMD.Run and INTE and waits for HCH within <see cref="XhciProtocol.HaltTimeoutMs"/>, a miss ignored. Thread context, from the detach hook with the hardware present.</summary>
    internal void Stop()
    {
        WriteOperational(XhciProtocol.UsbCmd, ReadOperational(XhciProtocol.UsbCmd) & ~(XhciProtocol.UsbCmdRun | XhciProtocol.UsbCmdInterrupterEnable));
        WaitForStatus(XhciProtocol.UsbStsHalted, XhciProtocol.UsbStsHalted, XhciProtocol.HaltTimeoutMs);
    }

    // --- Allocation ---

    /// <summary>
    /// The one allocation policy: a 64-bit capable controller takes any DMA
    /// memory (the binding throws when none is left); one without AC64
    /// takes memory below 4 GiB only and gets null when there is none.
    /// Thread context.
    /// </summary>
    /// <param name="length">Bytes wanted.</param>
    /// <param name="alignment">Alignment of the first byte.</param>
    /// <returns>The buffer, or null when a 32-bit controller found no memory below 4 GiB.</returns>
    /// <exception cref="InvalidOperationException">No pages left, or the binding is being torn down.</exception>
    internal DmaBuffer? AllocateDma(int length, int alignment)
    {
        if (_is64BitCapable)
        {
            return _binding.AllocateDma(length, alignment);
        }

        return _binding.TryAllocateDma(length, alignment, DmaConstraints.Addressable32Bit, out DmaBuffer? buffer) ? buffer : null;
    }

    /// <summary><see cref="AllocateDma"/> for the pools, where exhaustion is a null open rather than an exception. Thread context.</summary>
    private DmaBuffer? TryAllocatePooled(int length, int alignment)
    {
        try
        {
            return AllocateDma(length, alignment);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>Takes a free slot memory set or allocates one (four pages and an event), cleared. Thread context, the address path.</summary>
    /// <returns>The set, or null when no DMA memory was left.</returns>
    private XhciSlotMemory? TakeSlotMemory()
    {
        XhciSlotMemory? memory = null;
        using (Lock.Acquire())
        {
            int last = _freeSlotSets.Count - 1;
            if (last >= 0)
            {
                memory = _freeSlotSets[last];
                _freeSlotSets.RemoveAt(last);
            }
        }

        if (memory is null)
        {
            DmaBuffer? output = TryAllocatePooled(PageBytes, PageBytes);
            DmaBuffer? input = TryAllocatePooled(PageBytes, PageBytes);
            DmaBuffer? ring = TryAllocatePooled(PageBytes, PageBytes);
            DmaBuffer? buffer = TryAllocatePooled(PageBytes, PageBytes);
            if (output is null || input is null || ring is null || buffer is null)
            {
                return null;
            }

            memory = new XhciSlotMemory(output, input, ring, buffer, _binding.CreateEvent(), _contextSize);
        }

        memory.Clear();
        return memory;
    }

    /// <summary>Takes a free pipe memory set of one kind or allocates one, its ring reset. Thread context, the open path.</summary>
    /// <param name="bulk">True for the 64 KiB bounce and an event, false for one buffer page.</param>
    /// <returns>The set, or null when no DMA memory was left.</returns>
    private XhciPipeMemory? TakePipeMemory(bool bulk)
    {
        List<XhciPipeMemory> pool = bulk ? _freeBulkPipeMemory : _freeInterruptPipeMemory;
        XhciPipeMemory? memory = null;
        using (Lock.Acquire())
        {
            int last = pool.Count - 1;
            if (last >= 0)
            {
                memory = pool[last];
                pool.RemoveAt(last);
            }
        }

        if (memory is null)
        {
            DmaBuffer? ring = TryAllocatePooled(PageBytes, PageBytes);
            DmaBuffer? buffer = bulk
                ? TryAllocatePooled(XhciProtocol.BulkBufferBytes, XhciProtocol.BulkBufferBytes)
                : TryAllocatePooled(PageBytes, PageBytes);
            if (ring is null || buffer is null)
            {
                return null;
            }

            memory = new XhciPipeMemory(ring, buffer, bulk ? _binding.CreateEvent() : null);
        }

        memory.Ring.Reset();
        return memory;
    }

    /// <summary>Returns a pipe memory set to its pool. Under the lock.</summary>
    private void ReturnPipeMemory(XhciPipeMemory memory)
    {
        if (memory.Event is null)
        {
            _freeInterruptPipeMemory.Add(memory);
        }
        else
        {
            _freeBulkPipeMemory.Add(memory);
        }
    }

    // --- Waits ---

    /// <summary>
    /// Waits for a completion flag the handler or the polled drain sets:
    /// true at once when it is set; false when the slot is disconnected or
    /// the deadline passed. With an interrupt, and while the host binding
    /// is not detaching, the thread parks on <paramref name="evt"/> for the
    /// remaining time and re-checks the flag after every wake; otherwise it
    /// drains the event ring under the lock between delays, which is how
    /// the pipe closes and the Disable Slot of a teardown complete after
    /// the kit cancelled the binding's events. The lock is never held
    /// across the wait. Thread context.
    /// </summary>
    /// <param name="completed">The flag, a plain field written with <c>Volatile</c>.</param>
    /// <param name="evt">The event the completion signals.</param>
    /// <param name="slot">The slot whose disconnection ends the wait, or null for a command.</param>
    /// <param name="timeoutMs">The budget.</param>
    private bool WaitCompletion(ref bool completed, DeviceEvent evt, XhciSlot? slot, uint timeoutMs)
    {
        long deadline = DeadlineAfter(timeoutMs);
        while (true)
        {
            if (Volatile.Read(ref completed))
            {
                return true;
            }

            if (slot is { IsDisconnected: true })
            {
                return false;
            }

            long now = Stopwatch.GetTimestamp();
            if (now >= deadline)
            {
                return false;
            }

            if (_hasInterrupt && !_binding.IsDetaching)
            {
                _binding.Wait(evt, RemainingMilliseconds(deadline, now));
            }
            else
            {
                using (Lock.Acquire())
                {
                    DrainEvents(null);
                }

                _binding.Delay(XhciProtocol.PollMicroseconds);
            }
        }
    }

    /// <summary>
    /// Claims the controller's one-command flag under the lock, in a delay
    /// loop bounded by <see cref="XhciProtocol.CommandTimeoutMs"/>. Thread
    /// context.
    /// </summary>
    /// <returns>False when the flag stayed held past the budget.</returns>
    private bool ClaimCommand()
    {
        long deadline = DeadlineAfter(XhciProtocol.CommandTimeoutMs);
        while (true)
        {
            using (Lock.Acquire())
            {
                if (!_commandBusy)
                {
                    _commandBusy = true;
                    return true;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(XhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>
    /// Claims a slot's control ring under the lock, in a delay loop bounded
    /// by <paramref name="timeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set. A recovery's CLEAR_FEATURE that
    /// still owns the ring at the deadline is the one holder without a
    /// timeout of its own (a device that never answers, or a stage the
    /// controller never fetched), so the ring is taken from it: its
    /// addresses cleared, its pipe stopped, the endpoint marked halted so
    /// the caller's transfer resets it and moves the dequeue pointer past
    /// the abandoned stages first. A late completion for them is dropped,
    /// both addresses being 0. Thread context.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="timeoutMs">The budget.</param>
    /// <returns>Success when claimed; Disconnected or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimControl(XhciSlot slot, uint timeoutMs)
    {
        long deadline = DeadlineAfter(timeoutMs);
        while (true)
        {
            if (slot.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            bool expired = Stopwatch.GetTimestamp() >= deadline;
            using (Lock.Acquire())
            {
                if (!slot.ControlBusy)
                {
                    slot.ControlBusy = true;
                    return UsbTransferStatus.Success;
                }

                if (expired && slot.RecoveryStatusTrb != 0)
                {
                    slot.RecoveryStatusTrb = 0;
                    if (slot.RecoveringPipe is { } recovering)
                    {
                        recovering.State = XhciPipeState.Stopped;
                    }

                    slot.RecoveringPipe = null;
                    slot.ControlEndpointHalted = true;
                    return UsbTransferStatus.Success;
                }
            }

            if (expired)
            {
                return UsbTransferStatus.Timeout;
            }

            _binding.Delay(XhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>
    /// Claims a bulk pipe under the lock, in a delay loop bounded by
    /// <see cref="XhciProtocol.BulkTimeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set. Thread context.
    /// </summary>
    /// <param name="slot">The pipe's slot.</param>
    /// <param name="pipe">The pipe.</param>
    /// <returns>Success when claimed; Disconnected or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimBulk(XhciSlot slot, XhciBulkPipe pipe)
    {
        long deadline = DeadlineAfter(XhciProtocol.BulkTimeoutMs);
        while (true)
        {
            if (slot.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            using (Lock.Acquire())
            {
                if (!pipe.Busy)
                {
                    pipe.Busy = true;
                    return UsbTransferStatus.Success;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return UsbTransferStatus.Timeout;
            }

            _binding.Delay(XhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a bulk pipe's claim under the lock. Thread context.</summary>
    private void ReleaseBulk(XhciBulkPipe pipe)
    {
        using (Lock.Acquire())
        {
            pipe.Busy = false;
        }
    }

    /// <summary>
    /// Claims a slot's input context under the lock, in a delay loop bounded
    /// by <see cref="XhciProtocol.CommandTimeoutMs"/>: the page is written
    /// and then read by one Configure Endpoint or Evaluate Context, and two
    /// callers on different threads (a ring thread's endpoint reset, the
    /// worker's pipe open or close for a sibling endpoint) must not
    /// interleave their writes. Thread context.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>False when the flag stayed held past the budget, which the caller treats as a failed command.</returns>
    private bool ClaimInput(XhciSlot slot)
    {
        long deadline = DeadlineAfter(XhciProtocol.CommandTimeoutMs);
        while (true)
        {
            using (Lock.Acquire())
            {
                if (!slot.InputBusy)
                {
                    slot.InputBusy = true;
                    return true;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(XhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a slot's input context claim under the lock. Thread context.</summary>
    private void ReleaseInput(XhciSlot slot)
    {
        using (Lock.Acquire())
        {
            slot.InputBusy = false;
        }
    }

    /// <summary>
    /// Waits <paramref name="milliseconds"/> in 1 ms delay steps, leaving
    /// early when the host binding begins detaching, so the hot-plug thread
    /// leaves within one step of the teardown flag. Thread context.
    /// </summary>
    /// <param name="milliseconds">How long.</param>
    /// <returns>False when the binding is detaching.</returns>
    private bool SettleMilliseconds(uint milliseconds)
    {
        for (uint waited = 0; waited < milliseconds; waited++)
        {
            if (_binding.IsDetaching)
            {
                return false;
            }

            _binding.Delay(XhciProtocol.MicrosecondsPerMillisecond);
        }

        return !_binding.IsDetaching;
    }

    /// <summary>Polls USBSTS every 1 ms until <c>(USBSTS &amp; mask) == expected</c>, up to <paramref name="timeoutMs"/>. Thread context.</summary>
    private bool WaitForStatus(uint mask, uint expected, uint timeoutMs)
    {
        for (uint waitedMs = 0; ; waitedMs++)
        {
            if ((ReadOperational(XhciProtocol.UsbSts) & mask) == expected)
            {
                return true;
            }

            if (waitedMs >= timeoutMs)
            {
                return false;
            }

            _binding.Delay(XhciProtocol.MicrosecondsPerMillisecond);
        }
    }

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    private static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;

    /// <summary>Milliseconds from <paramref name="now"/> to <paramref name="deadline"/>, at least 1. Any context.</summary>
    private static uint RemainingMilliseconds(long deadline, long now) =>
        (uint)Math.Max(1, (deadline - now) * MillisecondsPerSecond / Stopwatch.Frequency);

    // --- Registers ---

    /// <summary>Reads an operational register. Any context.</summary>
    private uint ReadOperational(ulong offset) => _registers.Read32(_operationalBase + offset);

    /// <summary>Writes an operational register. Any context.</summary>
    private void WriteOperational(ulong offset, uint value) => _registers.Write32(_operationalBase + offset, value);

    /// <summary>Writes a 64-bit operational register, low dword first. Thread context.</summary>
    private void WriteOperational64(ulong offset, ulong value)
    {
        _registers.Write32(_operationalBase + offset, (uint)value);
        _registers.Write32(_operationalBase + offset + XhciProtocol.DwordBytes, (uint)(value >> UpperDwordShift));
    }

    /// <summary>Writes a register of interrupter 0. Any context.</summary>
    private void WriteInterrupter(ulong offset, uint value) =>
        _registers.Write32(_runtimeBase + XhciProtocol.Interrupter0Offset + offset, value);

    /// <summary>Writes a 64-bit register of interrupter 0, low dword first. Any context.</summary>
    private void WriteInterrupter64(ulong offset, ulong value)
    {
        _registers.Write32(_runtimeBase + XhciProtocol.Interrupter0Offset + offset, (uint)value);
        _registers.Write32(_runtimeBase + XhciProtocol.Interrupter0Offset + offset + XhciProtocol.DwordBytes, (uint)(value >> UpperDwordShift));
    }

    /// <summary>Reads PORTSC of a 1-based root port. Any context.</summary>
    private uint ReadPortSc(byte port) =>
        _registers.Read32(_operationalBase + XhciProtocol.PortRegisterSetOffset + ((ulong)(port - 1) * XhciProtocol.PortRegisterSetStride));

    /// <summary>Writes PORTSC of a 1-based root port. Thread context.</summary>
    private void WritePortSc(byte port, uint value) =>
        _registers.Write32(_operationalBase + XhciProtocol.PortRegisterSetOffset + ((ulong)(port - 1) * XhciProtocol.PortRegisterSetStride), value);

    /// <summary>
    /// Rings doorbell <paramref name="slotId"/> (0 is the command ring) with
    /// <paramref name="target"/> (0 for commands, the endpoint's DCI
    /// otherwise). The window's write barrier orders the ring stores before
    /// it. Any context.
    /// </summary>
    private void RingDoorbell(byte slotId, uint target) =>
        _registers.Write32(_doorbellBase + (slotId * XhciProtocol.DoorbellStride), target);

    /// <summary>
    /// Byte offset from BAR0 of the next extended capability with
    /// <paramref name="id"/> after the one at <paramref name="after"/> (0 to
    /// start from the first), or 0 when there is none. Thread context.
    /// </summary>
    private ulong FindExtendedCapability(byte id, ulong after)
    {
        ulong capability = after == 0 ? _extendedCapabilities : NextExtendedCapability(after);
        while (capability != 0)
        {
            if ((_registers.Read32(capability) & XhciProtocol.ExtendedCapabilityIdMask) == id)
            {
                return capability;
            }

            capability = NextExtendedCapability(capability);
        }

        return 0;
    }

    /// <summary>The capability after <paramref name="capability"/>, or 0 at the end of the list.</summary>
    private ulong NextExtendedCapability(ulong capability)
    {
        uint next = (_registers.Read32(capability) >> XhciProtocol.ExtendedCapabilityNextShift) & XhciProtocol.ExtendedCapabilityNextMask;
        return next == 0 ? 0 : capability + ((ulong)next << XhciProtocol.DwordShift);
    }
}
