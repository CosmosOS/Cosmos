// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// Everything <see cref="EhciDriver"/> holds for one bound controller, hung
/// off <see cref="DeviceBinding.DriverState"/>, and the kit's
/// <see cref="UsbHostController"/> for it: the register window and its two
/// register sets, the periodic frame list with its interval heads, the
/// async schedule with its head, the descriptor pool, the device table,
/// the pooled device and pipe memory sets, the kit lock, the interrupt
/// outcome and the counters the suites read. Control and bulk transfers
/// are synchronous and run in thread context; completions are found by
/// reading the descriptors back, by the line interrupt handler (no lock: a
/// holder of the <see cref="DeviceLock"/> runs with interrupts disabled,
/// so the two never overlap) or, on a polled controller and while the host
/// binding detaches, by the waiting thread and the hot-plug thread under
/// the lock. The lock guards every schedule change, every descriptor write
/// the controller may be reading, the device table, the pipe tables and
/// the pools, and is never held across a wait, a delay, a sleep or a kit
/// call other than a report delivery on a polled controller. The busy
/// flags (a device's <see cref="EhciDevice.ControlBusy"/>, a bulk pipe's
/// <see cref="EhciBulkPipe.Busy"/>, the controller's default address and
/// its doorbell) are claimed under the lock in a delay loop bounded by the
/// operation's own timeout. The partials hold the devices, the control
/// transfers, the bulk transfers, the interrupt pipes, the schedule, the
/// events and the ports.
/// </summary>
internal sealed partial class EhciState : UsbHostController
{
    // --- Constants ---

    /// <summary>Bytes of one page: the frame list, every pool page and every buffer page is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>The probe's failure text when no DMA memory below 4 GiB is left; every data structure of the controller lives there.</summary>
    private const string NoLowMemoryFailure = "no DMA memory below 4 GiB";

    /// <summary>Pipes one recovery pass of the hot-plug thread snapshots at most; the rest wait for the next pass.</summary>
    private const int RecoverySnapshotLength = 64;

    /// <summary>Capability list entries the firmware hand-off walks at most.</summary>
    private const int MaxCapabilityWalk = 48;

    /// <summary>Capability offsets are dword aligned.</summary>
    private const byte CapabilityOffsetMask = 0xFC;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly ulong _operationalBase;
    private readonly ushort _version;
    private readonly byte _portCount;
    private readonly byte _companionCount;
    private readonly byte _companionPortCount;
    private readonly bool _hasPortPowerControl;
    private readonly bool _hasPortRoutingRules;
    private readonly byte[] _portRoute;
    private readonly bool _is64BitCapable;
    private readonly byte _extendedCapabilities;
    private readonly EhciDevice?[] _devices = new EhciDevice?[EhciProtocol.AddressCount];
    private readonly bool[] _addressTaken = new bool[EhciProtocol.AddressCount];
    private readonly EhciDescriptorPool _pool = new();
    private readonly List<EhciDeviceMemory> _freeDeviceSets = [];
    private readonly List<EhciPipeMemory> _freeInterruptPipeMemory = [];
    private readonly List<EhciPipeMemory> _freeBulkPipeMemory = [];
    private readonly List<EhciDescriptorSlot> _retiredSlots = [];
    private readonly List<EhciQueueHeadSlot> _asyncQueueHeads = [];
    private readonly List<EhciQueueHeadSlot>[] _periodicQueueHeads = new List<EhciQueueHeadSlot>[EhciProtocol.IntervalHeadCount];
    private readonly EhciInterruptPipe?[] _recoverySnapshot = new EhciInterruptPipe?[RecoverySnapshotLength];
    private readonly DeviceEvent _doorbellEvent;
    private readonly DeviceEvent _portChangeEvent;
    private readonly WorkItem _faultReport;
    private DmaBuffer? _frameList;
    private EhciQueueHeadSlot? _asyncHead;
    private EhciQueueHeadSlot[] _intervalHeads = [];
    private DeviceLock? _lock;
    private UsbBus? _bus;
    private volatile bool _running;
    private volatile bool _rootPortsProbed;
    private volatile bool _recoveryRequested;

    // The default address claim: one device answers at address 0 at a time,
    // held for the whole address path. The doorbell claim: one async unlink
    // at a time owns the predecessor write, the IAAD write and the IAA wait.
    private bool _addressZeroBusy;
    private bool _doorbellBusy;
    private bool _doorbellCompleted;
    private bool _doorbellTimeoutLogged;

    // What the handler saw and could not log: the fault report work item
    // logs it in thread context.
    private volatile bool _transferFaultPending;
    private byte _transferFaultAddress;
    private byte _transferFaultEndpoint;
    private uint _transferFaultToken;
    private volatile bool _pipeStopPending;
    private byte _pipeStopAddress;
    private byte _pipeStopEndpoint;
    private volatile bool _hostErrorPending;

    private int _index;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile bool _hotPlugRunning;
    private volatile int _timeouts;
    private volatile int _devicesAddressed;
    private volatile int _pipesClosed;
    private volatile int _interruptCount;
    private volatile int _pipeRecoveries;
    private volatile int _pipesStopped;
    private volatile int _hostControllerEvents;
    private volatile int _companionPortsReleased;
    private volatile int _doorbellTimeouts;
    private volatile int _lostDoorbellInterrupts;

    // --- Constructor ---

    /// <summary>
    /// Reads the capability registers (CAPLENGTH, HCIVERSION, HCSPARAMS,
    /// HCCPARAMS and, with port routing rules, HCSP-PORTROUTE), locates
    /// the operational register set and creates the doorbell and port
    /// change events and the fault report work item. The schedules come
    /// through <see cref="AllocateStructures"/> and the lock through
    /// <see cref="Lock"/>. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The function's binding, for the DMA memory, the events, the delays, the waits and the log.</param>
    /// <param name="registers">The BAR0 window.</param>
    internal EhciState(DeviceBinding binding, RegisterWindow registers)
    {
        _binding = binding;
        _registers = registers;

        _operationalBase = registers.Read8(EhciProtocol.CapLength);
        _version = registers.Read16(EhciProtocol.HciVersion);

        uint hcsParams = registers.Read32(EhciProtocol.HcsParams);
        _portCount = (byte)(hcsParams & EhciProtocol.HcsParamsPortCountMask);
        _hasPortPowerControl = (hcsParams & EhciProtocol.HcsParamsPortPowerControl) != 0;
        _hasPortRoutingRules = (hcsParams & EhciProtocol.HcsParamsPortRoutingRules) != 0;
        _companionPortCount = (byte)((hcsParams >> EhciProtocol.HcsParamsCompanionPortCountShift) & EhciProtocol.HcsParamsCompanionPortCountMask);
        _companionCount = (byte)((hcsParams >> EhciProtocol.HcsParamsCompanionCountShift) & EhciProtocol.HcsParamsCompanionCountMask);

        uint hccParams = registers.Read32(EhciProtocol.HccParams);
        _is64BitCapable = (hccParams & EhciProtocol.HccParamsAddressing64) != 0;
        _extendedCapabilities = (byte)((hccParams >> EhciProtocol.HccParamsExtendedCapabilitiesShift) & EhciProtocol.HccParamsExtendedCapabilitiesMask);

        _portRoute = new byte[_portCount];
        if (_hasPortRoutingRules && _operationalBase >= EhciProtocol.HcspPortRoute + EhciProtocol.HcspPortRouteBytes)
        {
            ulong route = registers.Read32(EhciProtocol.HcspPortRoute)
                | ((ulong)registers.Read32(EhciProtocol.HcspPortRoute + EhciProtocol.DwordBytes) << 32);
            for (int port = 0; port < _portCount; port++)
            {
                _portRoute[port] = (byte)((route >> (EhciProtocol.PortRouteNibbleBits * port)) & EhciProtocol.PortRouteNibbleMask);
            }
        }

        for (int i = 0; i < _periodicQueueHeads.Length; i++)
        {
            _periodicQueueHeads[i] = [];
        }

        _doorbellEvent = binding.CreateEvent();
        _portChangeEvent = binding.CreateEvent();
        _faultReport = binding.CreateWorkItem(ReportFaults);
    }

    // --- Properties the suites read ---

    /// <inheritdoc/>
    public override string Name => "EHCI";

    /// <summary>The controller's number, 0 for the first one bound; assigned when the probe binds. Any context.</summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>HCIVERSION, 0x100 for a 1.0 controller. Any context.</summary>
    public ushort Version => _version;

    /// <summary>HCSPARAMS.N_PORTS: root ports. Any context.</summary>
    public byte PortCount => _portCount;

    /// <summary>HCSPARAMS.N_CC: companion controllers the full and low-speed devices are handed to. Any context.</summary>
    public byte CompanionCount => _companionCount;

    /// <summary>HCSPARAMS.N_PCC: ports per companion controller. Any context.</summary>
    public byte CompanionPortCount => _companionPortCount;

    /// <summary>True when the legacy line interrupt is connected to <see cref="OnInterrupt"/>, so a wait parks on an event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when no line interrupt could be connected, so the waiting thread and the hot-plug thread read the status and the descriptors themselves. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True once the <c>ehci-hotplug</c> thread started; false without a scheduler, when only boot-time devices are enumerated. Any context.</summary>
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

    /// <summary>How many control or bulk transfers hit their budget. Any context.</summary>
    public int Timeouts => _timeouts;

    /// <summary>How many devices SET_ADDRESS gave an address and a descriptor. Any context.</summary>
    public int DevicesAddressed => _devicesAddressed;

    /// <summary>How many pipes were closed, by a driver or by a binding's unwind. Any context.</summary>
    public int PipesClosed => _pipesClosed;

    /// <summary>How many times the line interrupt ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many interrupt pipe recoveries were started after a transfer error. Any context.</summary>
    public int PipeRecoveries => _pipeRecoveries;

    /// <summary>How many interrupt pipes were stopped for good after errors, with nobody to recover them or past the recovery limit. Any context.</summary>
    public int PipesStopped => _pipesStopped;

    /// <summary>How many host system errors halted the controller. Any context.</summary>
    public int HostControllerEvents => _hostControllerEvents;

    /// <summary>How many root ports were handed to a companion controller for a full or low-speed device. Any context.</summary>
    public int CompanionPortsReleased => _companionPortsReleased;

    /// <summary>How many async doorbells went unanswered within <see cref="EhciProtocol.DoorbellTimeoutMs"/>, each retiring a queue head. Any context.</summary>
    public int DoorbellTimeouts => _doorbellTimeouts;

    /// <summary>How many async doorbell acknowledges the wait found in the registers rather than through the line or the polled service: IAA set without the line raised, or IAAD cleared without IAA set, read back every <see cref="EhciProtocol.DoorbellWatchdogMs"/>. Any context.</summary>
    public int LostDoorbellInterrupts => _lostDoorbellInterrupts;

    // --- Internal properties the probe reads and sets ---

    /// <summary>The lock every schedule change, descriptor hand-over and table change runs under; set by the probe before the handler is connected.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the lock.</exception>
    internal DeviceLock Lock
    {
        get => _lock ?? throw new InvalidOperationException("The controller's lock is not created.");
        set => _lock = value;
    }

    /// <summary>HCCPARAMS.AC64: the controller takes 64-bit data structure addresses; CTRLDSSEGMENT is then written 0, every structure staying below 4 GiB.</summary>
    internal bool Is64BitCapable => _is64BitCapable;

    /// <summary>HCSPARAMS.PPC: software switches port power.</summary>
    internal bool HasPortPowerControl => _hasPortPowerControl;

    /// <summary>Bytes from BAR0 the driver touches: the last port register.</summary>
    internal ulong RequiredRegisterLength => _operationalBase + EhciProtocol.PortScOffset + (EhciProtocol.PortScStride * _portCount);

    /// <summary>True from the controller's start to its stop, cleared early by a host system error: the window in which the schedules are worth touching. Any context.</summary>
    internal bool Running
    {
        get => _running;
        set => _running = value;
    }

    /// <summary>
    /// <see cref="Running"/> and USBSTS.HCH clear: a halted controller
    /// fetches nothing, and a PCI function that vanished reads all ones,
    /// HCH included, so a pipe close or a quiesce against either skips the
    /// doorbell and the frame settles instead of waiting out their
    /// budgets. Any context.
    /// </summary>
    private bool ControllerRuns => _running && (ReadOperational(EhciProtocol.UsbSts) & EhciProtocol.UsbStsHalted) == 0;

    /// <summary>The frame list, once allocated.</summary>
    private DmaBuffer FrameList => _frameList ?? throw new InvalidOperationException("The frame list is not allocated.");

    /// <summary>The async schedule's head, once allocated.</summary>
    private EhciQueueHeadSlot AsyncHead => _asyncHead ?? throw new InvalidOperationException("The async head is not allocated.");

    // --- Internal methods for the probe and the detach hook ---

    /// <summary>
    /// Allocates the frame list, the descriptor pool's first page, the
    /// async head and the eight interval heads, all below 4 GiB, and
    /// builds both schedules in memory (EHCI 1.0 sections 4.7 and 4.8):
    /// nothing after this allocates before the controller is configured.
    /// Thread context, from the probe, with the lock created.
    /// </summary>
    /// <returns>Null when everything was allocated; the probe's failure text when no memory below 4 GiB was found.</returns>
    internal string? AllocateStructures()
    {
        DmaBuffer? frameList = AllocateDma(EhciProtocol.FrameListBytes, PageBytes);
        if (frameList is null)
        {
            return NoLowMemoryFailure;
        }

        _frameList = frameList;

        EhciDescriptorSlot[] slots = new EhciDescriptorSlot[EhciProtocol.IntervalHeadCount + 1];
        if (!TryTakeSlots(slots))
        {
            return NoLowMemoryFailure;
        }

        _asyncHead = new EhciQueueHeadSlot(slots[0]);
        EhciQueueHeadSlot[] heads = new EhciQueueHeadSlot[EhciProtocol.IntervalHeadCount];
        for (int i = 0; i < heads.Length; i++)
        {
            heads[i] = new EhciQueueHeadSlot(slots[i + 1]);
        }

        _intervalHeads = heads;
        BuildSchedules();
        return null;
    }

    /// <summary>
    /// Asks the firmware to hand the controller over through the USB Legacy
    /// Support capability in PCI configuration space (EHCI 1.0 sections
    /// 2.1.7 and 5.1): the capability list is walked from HCCPARAMS.EECP;
    /// with BIOS Owned set, OS Owned is set with a byte write (a dword
    /// write could re-set the BIOS bit the SMI handler just cleared) and
    /// BIOS Owned polled clear for up to <see cref="EhciProtocol.FirmwareHandoffTimeoutMs"/>,
    /// else logged and cleared; then USBLEGCTLSTS is written 0 so no SMI
    /// source stays armed. Thread context, from the probe.
    /// </summary>
    /// <param name="pci">The function's configuration space.</param>
    internal void TakeOwnershipFromFirmware(PciAccess pci)
    {
        if (_extendedCapabilities < EhciProtocol.LegacyCapabilityMinOffset)
        {
            return;
        }

        byte offset = (byte)(_extendedCapabilities & CapabilityOffsetMask);
        bool found = false;
        for (int walked = 0; walked < MaxCapabilityWalk && offset >= EhciProtocol.LegacyCapabilityMinOffset; walked++)
        {
            if (pci.ReadConfig8(offset) == EhciProtocol.LegacySupportCapabilityId)
            {
                found = true;
                break;
            }

            offset = (byte)(pci.ReadConfig8((ushort)(offset + EhciProtocol.CapabilityNextPointerOffset)) & CapabilityOffsetMask);
        }

        if (!found)
        {
            return;
        }

        if ((pci.ReadConfig32(offset) & EhciProtocol.LegacyBiosOwned) != 0)
        {
            pci.WriteConfig8((ushort)(offset + EhciProtocol.LegacyOsOwnedByteOffset), 1);
            uint waitedMs = 0;
            while ((pci.ReadConfig32(offset) & EhciProtocol.LegacyBiosOwned) != 0 && waitedMs < EhciProtocol.FirmwareHandoffTimeoutMs)
            {
                _binding.Delay(EhciProtocol.MicrosecondsPerMillisecond);
                waitedMs++;
            }

            if ((pci.ReadConfig32(offset) & EhciProtocol.LegacyBiosOwned) != 0)
            {
                _binding.Log("firmware did not release the controller, taking it over");
                pci.WriteConfig8((ushort)(offset + EhciProtocol.LegacyBiosOwnedByteOffset), 0);
            }
        }

        pci.WriteConfig32((ushort)(offset + EhciProtocol.LegacyControlStatusOffset), 0);
    }

    /// <summary>
    /// Stops the controller and resets it (EHCI 1.0 section 2.3.1): RS
    /// cleared and HCH awaited within <see cref="EhciProtocol.HaltTimeoutMs"/>,
    /// HCRESET set and its self-clear awaited. From here until CONFIGFLAG
    /// is written every port is routed to the companions, so only register
    /// writes follow. Thread context, from the probe.
    /// </summary>
    /// <returns>Null when the reset completed; the probe's failure text otherwise.</returns>
    internal string? Reset()
    {
        WriteOperational(EhciProtocol.UsbCmd, ReadOperational(EhciProtocol.UsbCmd) & ~EhciProtocol.UsbCmdRun);
        if (!WaitForStatus(EhciProtocol.UsbStsHalted, EhciProtocol.UsbStsHalted, EhciProtocol.HaltTimeoutMs))
        {
            return "the controller did not halt";
        }

        WriteOperational(EhciProtocol.UsbCmd, ReadOperational(EhciProtocol.UsbCmd) | EhciProtocol.UsbCmdReset);
        uint waitedMs = 0;
        while ((ReadOperational(EhciProtocol.UsbCmd) & EhciProtocol.UsbCmdReset) != 0)
        {
            if (waitedMs++ >= EhciProtocol.ResetTimeoutMs)
            {
                return "the controller reset did not complete";
            }

            _binding.Delay(EhciProtocol.MicrosecondsPerMillisecond);
        }

        return null;
    }

    /// <summary>
    /// Programs the reset controller and starts it (EHCI 1.0 section 4.1):
    /// CTRLDSSEGMENT 0 on a 64-bit controller, PERIODICLISTBASE,
    /// ASYNCLISTADDR, FRINDEX 0, every status bit cleared, every interrupt
    /// enable off, then USBCMD with a one micro-frame interrupt threshold,
    /// the 1024-entry frame list, both schedules enabled and RS set, and
    /// HCH awaited clear within <see cref="EhciProtocol.HaltTimeoutMs"/>.
    /// A miss clears RS again and awaits HCH: the probe then fails and the
    /// kit frees the schedules, which a controller still commanded to run
    /// would walk. Thread context, from the probe.
    /// </summary>
    /// <returns>False when the controller did not start.</returns>
    internal bool Start()
    {
        if (_is64BitCapable)
        {
            WriteOperational(EhciProtocol.CtrlDsSegment, 0);
        }

        WriteOperational(EhciProtocol.PeriodicListBase, (uint)FrameList.PhysicalAddress);
        WriteOperational(EhciProtocol.AsyncListAddr, (uint)AsyncHead.PhysicalAddress);
        WriteOperational(EhciProtocol.FrIndex, 0);
        WriteOperational(EhciProtocol.UsbSts, EhciProtocol.UsbStsClearMask);
        WriteOperational(EhciProtocol.UsbIntr, 0);

        uint command = (EhciProtocol.InterruptThresholdOneMicroFrame << EhciProtocol.UsbCmdInterruptThresholdShift)
            | EhciProtocol.UsbCmdAsyncScheduleEnable
            | EhciProtocol.UsbCmdPeriodicScheduleEnable
            | EhciProtocol.UsbCmdRun;
        WriteOperational(EhciProtocol.UsbCmd, command);
        if (!WaitForStatus(EhciProtocol.UsbStsHalted, 0, EhciProtocol.HaltTimeoutMs))
        {
            WriteOperational(EhciProtocol.UsbCmd, ReadOperational(EhciProtocol.UsbCmd) & ~EhciProtocol.UsbCmdRun);
            WaitForStatus(EhciProtocol.UsbStsHalted, EhciProtocol.UsbStsHalted, EhciProtocol.HaltTimeoutMs);
            return false;
        }

        _running = true;
        return true;
    }

    /// <summary>Writes CONFIGFLAG so every root port is routed to this controller (EHCI 1.0 section 4.2); the per-port release decides again for each full or low-speed device. Thread context, from the probe.</summary>
    internal void Configure()
    {
        WriteOperational(EhciProtocol.ConfigFlag, EhciProtocol.ConfigFlagConfigured);
    }

    /// <summary>Turns on the interrupt enables the handler acts on, once the line interrupt is connected. Thread context, from the probe.</summary>
    internal void EnableInterrupts()
    {
        WriteOperational(EhciProtocol.UsbIntr, EhciProtocol.InterruptMask);
    }

    /// <summary>Turns every interrupt enable off, clears RS and waits for HCH within <see cref="EhciProtocol.HaltTimeoutMs"/>, a miss ignored. Thread context, from the detach hook or a failed probe with the hardware present.</summary>
    internal void Stop()
    {
        WriteOperational(EhciProtocol.UsbIntr, 0);
        WriteOperational(EhciProtocol.UsbCmd, ReadOperational(EhciProtocol.UsbCmd) & ~EhciProtocol.UsbCmdRun);
        WaitForStatus(EhciProtocol.UsbStsHalted, EhciProtocol.UsbStsHalted, EhciProtocol.HaltTimeoutMs);
    }

    // --- Allocation ---

    /// <summary>
    /// The one allocation policy: every data structure and buffer lives
    /// below 4 GiB, since the driver uses the 32-bit data structures and
    /// writes CTRLDSSEGMENT 0; null when no such memory is left. Thread
    /// context.
    /// </summary>
    /// <param name="length">Bytes wanted.</param>
    /// <param name="alignment">Alignment of the first byte.</param>
    /// <returns>The buffer, or null when no memory below 4 GiB was found.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down.</exception>
    internal DmaBuffer? AllocateDma(int length, int alignment)
    {
        return _binding.TryAllocateDma(length, alignment, DmaConstraints.Addressable32Bit, out DmaBuffer? buffer) ? buffer : null;
    }

    /// <summary><see cref="AllocateDma"/> for the pools, where a detaching binding is a null open rather than an exception. Thread context.</summary>
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

    /// <summary>Creates an event for a new memory set, or null once the binding is detaching. Thread context.</summary>
    private DeviceEvent? TryCreateEvent()
    {
        try
        {
            return _binding.CreateEvent();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>Takes a zeroed descriptor slot from the pool, adding a page when the free list is empty. Thread context.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns>False when no DMA memory was left for a new page.</returns>
    private bool TryTakeSlot(out EhciDescriptorSlot slot)
    {
        using (Lock.Acquire())
        {
            if (_pool.TryTake(out slot))
            {
                return true;
            }
        }

        DmaBuffer? page = TryAllocatePooled(EhciDescriptorPool.PageBytes, PageBytes);
        if (page is null)
        {
            slot = default;
            return false;
        }

        using (Lock.Acquire())
        {
            _pool.AddPage(page);
            return _pool.TryTake(out slot);
        }
    }

    /// <summary>Fills <paramref name="slots"/> from the pool; on a miss the slots already taken go back. Thread context.</summary>
    /// <param name="slots">Receives the slots.</param>
    /// <returns>False when no DMA memory was left.</returns>
    private bool TryTakeSlots(EhciDescriptorSlot[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!TryTakeSlot(out slots[i]))
            {
                using (Lock.Acquire())
                {
                    for (int taken = 0; taken < i; taken++)
                    {
                        _pool.Return(slots[taken]);
                    }
                }

                return false;
            }
        }

        return true;
    }

    /// <summary>Takes a free device memory set or allocates one (a queue head, four qTD slots, a SETUP slot, a bounce page and an event), cleared. Thread context, the address path.</summary>
    /// <returns>The set, or null when no DMA memory was left or the binding is detaching.</returns>
    private EhciDeviceMemory? TakeDeviceMemory()
    {
        EhciDeviceMemory? memory = null;
        using (Lock.Acquire())
        {
            int last = _freeDeviceSets.Count - 1;
            if (last >= 0)
            {
                memory = _freeDeviceSets[last];
                _freeDeviceSets.RemoveAt(last);
            }
        }

        if (memory is null)
        {
            EhciDescriptorSlot[] heads = new EhciDescriptorSlot[2];
            if (!TryTakeSlots(heads))
            {
                return null;
            }

            EhciDescriptorSlot[] transfers = new EhciDescriptorSlot[EhciProtocol.ControlTransferSlots];
            if (!TryTakeSlots(transfers))
            {
                ReturnSlots(heads);
                return null;
            }

            DmaBuffer? buffer = TryAllocatePooled(PageBytes, PageBytes);
            DeviceEvent? controlEvent = buffer is null ? null : TryCreateEvent();
            if (buffer is null || controlEvent is null)
            {
                ReturnSlots(heads);
                ReturnSlots(transfers);
                return null;
            }

            memory = new EhciDeviceMemory(new EhciQueueHeadSlot(heads[0]), transfers, heads[1], buffer, controlEvent);
        }

        memory.Clear();
        return memory;
    }

    /// <summary>Takes a free pipe memory set of one kind or allocates one, cleared. Thread context, the open path.</summary>
    /// <param name="bulk">True for two qTD slots, the 64 KiB bounce and an event; false for nine qTD slots and one buffer page.</param>
    /// <returns>The set, or null when no DMA memory was left or the binding is detaching.</returns>
    private EhciPipeMemory? TakePipeMemory(bool bulk)
    {
        List<EhciPipeMemory> pool = bulk ? _freeBulkPipeMemory : _freeInterruptPipeMemory;
        EhciPipeMemory? memory = null;
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
            EhciDescriptorSlot[] heads = new EhciDescriptorSlot[1];
            if (!TryTakeSlots(heads))
            {
                return null;
            }

            EhciDescriptorSlot[] transfers = new EhciDescriptorSlot[bulk ? EhciProtocol.BulkTransferSlots : EhciProtocol.InterruptTransferSlots];
            if (!TryTakeSlots(transfers))
            {
                ReturnSlots(heads);
                return null;
            }

            DmaBuffer? buffer = bulk
                ? TryAllocatePooled(EhciProtocol.BulkBufferBytes, EhciProtocol.BulkBufferBytes)
                : TryAllocatePooled(PageBytes, PageBytes);
            DeviceEvent? transferEvent = null;
            if (buffer is not null && bulk)
            {
                transferEvent = TryCreateEvent();
            }

            if (buffer is null || (bulk && transferEvent is null))
            {
                ReturnSlots(heads);
                ReturnSlots(transfers);
                return null;
            }

            memory = new EhciPipeMemory(new EhciQueueHeadSlot(heads[0]), transfers, buffer, transferEvent);
        }

        memory.Clear();
        return memory;
    }

    /// <summary>Gives descriptor slots back to the pool. Thread context.</summary>
    private void ReturnSlots(EhciDescriptorSlot[] slots)
    {
        using (Lock.Acquire())
        {
            for (int i = 0; i < slots.Length; i++)
            {
                _pool.Return(slots[i]);
            }
        }
    }

    /// <summary>Returns a device memory set to its pool, unless the controller may still hold its queue head. Under the lock.</summary>
    private void ReturnDeviceMemory(EhciDeviceMemory memory)
    {
        if (!memory.Retired)
        {
            _freeDeviceSets.Add(memory);
        }
    }

    /// <summary>Returns a pipe memory set to its pool, unless the controller may still hold its queue head. Under the lock.</summary>
    private void ReturnPipeMemory(EhciPipeMemory memory)
    {
        if (memory.Retired)
        {
            return;
        }

        if (memory.IsBulk)
        {
            _freeBulkPipeMemory.Add(memory);
        }
        else
        {
            _freeInterruptPipeMemory.Add(memory);
        }
    }

    /// <summary>
    /// Parks a device memory set whose queue head the controller may still
    /// hold (an async doorbell that was not acknowledged): a controller
    /// keeps a cached queue keyed by the queue head's address, and a
    /// reused slot would be fetched as the old endpoint. Every slot of the
    /// set goes on the retired list and is never pooled again. Under the
    /// lock.
    /// </summary>
    private void RetireDeviceMemory(EhciDeviceMemory memory)
    {
        if (memory.Retired)
        {
            return;
        }

        memory.Retired = true;
        _retiredSlots.Add(memory.QueueHead.Slot);
        _retiredSlots.Add(memory.Setup);
        for (int i = 0; i < memory.Transfers.Length; i++)
        {
            _retiredSlots.Add(memory.Transfers[i]);
        }
    }

    /// <summary>Parks a pipe memory set whose queue head the controller may still hold, as <see cref="RetireDeviceMemory"/>. Under the lock.</summary>
    private void RetirePipeMemory(EhciPipeMemory memory)
    {
        if (memory.Retired)
        {
            return;
        }

        memory.Retired = true;
        _retiredSlots.Add(memory.QueueHead.Slot);
        for (int i = 0; i < memory.Transfers.Length; i++)
        {
            _retiredSlots.Add(memory.Transfers[i]);
        }
    }

    // --- Waits ---

    /// <summary>
    /// Waits for a completion flag the handler or the polled drain sets:
    /// true at once when it is set; false when the controller stopped
    /// running (a host system error halted it), when the device is
    /// disconnected or the host binding is detaching (a transfer wait,
    /// one with a device, must not outlive the kit's thread join), or
    /// when the deadline passed. With an interrupt, and while the host
    /// binding is not detaching, the thread parks on <paramref name="evt"/>
    /// for the remaining time and re-checks the flag after every wake;
    /// otherwise it services the status and reads the descriptors under
    /// the lock between delays, which is how the unlinks of a teardown
    /// complete after the kit cancelled the binding's events. The lock is
    /// never held across the wait. Thread context.
    /// </summary>
    /// <param name="completed">The flag, a plain field written with <c>Volatile</c>.</param>
    /// <param name="evt">The event the completion signals.</param>
    /// <param name="device">The device whose disconnection ends the wait, or null for a controller wait (the doorbell), bound by its budget alone.</param>
    /// <param name="timeoutMs">The budget.</param>
    private bool WaitCompletion(ref bool completed, DeviceEvent evt, EhciDevice? device, uint timeoutMs)
    {
        long deadline = DeadlineAfter(timeoutMs);
        while (true)
        {
            if (Volatile.Read(ref completed))
            {
                return true;
            }

            if (!_running)
            {
                return false;
            }

            if (device is not null && (device.IsDisconnected || _binding.IsDetaching))
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
                Poll();
                _binding.Delay(EhciProtocol.PollMicroseconds);
            }
        }
    }

    /// <summary>One polled pass under the lock: the status bits the handler would have seen, then every descriptor a thread or a pipe waits on. Thread context.</summary>
    private void Poll()
    {
        using (Lock.Acquire())
        {
            ServiceStatus(null);
            DrainCompletions(null);
        }
    }

    /// <summary>
    /// Claims a device's control queue head under the lock, in a delay
    /// loop bounded by <paramref name="timeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set. Thread context.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="timeoutMs">The budget.</param>
    /// <returns>Success when claimed; Disconnected or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimControl(EhciDevice device, uint timeoutMs)
    {
        long deadline = DeadlineAfter(timeoutMs);
        while (true)
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            using (Lock.Acquire())
            {
                if (!device.ControlBusy)
                {
                    device.ControlBusy = true;
                    return UsbTransferStatus.Success;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return UsbTransferStatus.Timeout;
            }

            _binding.Delay(EhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a device's control claim under the lock. Thread context.</summary>
    private void ReleaseControl(EhciDevice device)
    {
        using (Lock.Acquire())
        {
            device.ControlBusy = false;
        }
    }

    /// <summary>
    /// Claims a bulk pipe under the lock, in a delay loop bounded by
    /// <see cref="EhciProtocol.BulkTimeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set. Thread context.
    /// </summary>
    /// <param name="device">The pipe's device.</param>
    /// <param name="pipe">The pipe.</param>
    /// <returns>Success when claimed; Disconnected or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimBulk(EhciDevice device, EhciBulkPipe pipe)
    {
        long deadline = DeadlineAfter(EhciProtocol.BulkTimeoutMs);
        while (true)
        {
            if (device.IsDisconnected)
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

            _binding.Delay(EhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a bulk pipe's claim under the lock. Thread context.</summary>
    private void ReleaseBulk(EhciBulkPipe pipe)
    {
        using (Lock.Acquire())
        {
            pipe.Busy = false;
        }
    }

    /// <summary>
    /// Claims the default address under the lock, in a delay loop bounded
    /// by <see cref="EhciProtocol.AddressClaimTimeoutMs"/>: only one device
    /// may answer at address 0, and the kit may enumerate a root port and
    /// a hub port at the same time. Thread context, the address path.
    /// </summary>
    /// <returns>False when the flag stayed held past the budget.</returns>
    private bool ClaimAddressZero()
    {
        long deadline = DeadlineAfter(EhciProtocol.AddressClaimTimeoutMs);
        while (true)
        {
            using (Lock.Acquire())
            {
                if (!_addressZeroBusy)
                {
                    _addressZeroBusy = true;
                    return true;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(EhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases the default address claim under the lock. Thread context.</summary>
    private void ReleaseAddressZero()
    {
        using (Lock.Acquire())
        {
            _addressZeroBusy = false;
        }
    }

    /// <summary>
    /// Claims the async doorbell under the lock, in a delay loop bounded by
    /// <see cref="EhciProtocol.BulkTimeoutMs"/>: one unlink at a time owns
    /// the predecessor write, the IAAD write and the IAA wait, so two
    /// unlinkers never share one acknowledge. Thread context.
    /// </summary>
    /// <returns>False when the flag stayed held past the budget.</returns>
    private bool ClaimDoorbell()
    {
        long deadline = DeadlineAfter(EhciProtocol.BulkTimeoutMs);
        while (true)
        {
            using (Lock.Acquire())
            {
                if (!_doorbellBusy)
                {
                    _doorbellBusy = true;
                    return true;
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(EhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases the doorbell claim under the lock. Thread context.</summary>
    private void ReleaseDoorbell()
    {
        using (Lock.Acquire())
        {
            _doorbellBusy = false;
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

            _binding.Delay(EhciProtocol.MicrosecondsPerMillisecond);
        }

        return !_binding.IsDetaching;
    }

    /// <summary>Waits whole frames unconditionally, for a queue head leaving the periodic schedule: the controller must be off it before any dword is written. Thread context.</summary>
    /// <param name="frames">How many 1 ms frames.</param>
    private void WaitFrames(uint frames)
    {
        for (uint waited = 0; waited < frames; waited++)
        {
            _binding.Delay(EhciProtocol.MicrosecondsPerMillisecond);
        }
    }

    /// <summary>Polls USBSTS every 1 ms until <c>(USBSTS &amp; mask) == expected</c>, up to <paramref name="timeoutMs"/>. Thread context.</summary>
    private bool WaitForStatus(uint mask, uint expected, uint timeoutMs)
    {
        for (uint waitedMs = 0; ; waitedMs++)
        {
            if ((ReadOperational(EhciProtocol.UsbSts) & mask) == expected)
            {
                return true;
            }

            if (waitedMs >= timeoutMs)
            {
                return false;
            }

            _binding.Delay(EhciProtocol.MicrosecondsPerMillisecond);
        }
    }

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    private static long DeadlineAfter(uint milliseconds)
    {
        return Stopwatch.GetTimestamp() + (Stopwatch.Frequency / MillisecondsPerSecond * milliseconds);
    }

    /// <summary>Milliseconds from <paramref name="now"/> to <paramref name="deadline"/>, at least 1. Any context.</summary>
    private static uint RemainingMilliseconds(long deadline, long now)
    {
        return (uint)Math.Max(1, (deadline - now) * MillisecondsPerSecond / Stopwatch.Frequency);
    }

    // --- Registers ---

    /// <summary>Reads an operational register. Any context.</summary>
    private uint ReadOperational(ulong offset)
    {
        return _registers.Read32(_operationalBase + offset);
    }

    /// <summary>Writes an operational register. Any context.</summary>
    private void WriteOperational(ulong offset, uint value)
    {
        _registers.Write32(_operationalBase + offset, value);
    }

    /// <summary>Reads PORTSC of a 1-based root port. Any context.</summary>
    private uint ReadPortSc(byte port)
    {
        return _registers.Read32(_operationalBase + EhciProtocol.PortScOffset + ((ulong)(port - 1) * EhciProtocol.PortScStride));
    }

    /// <summary>Writes PORTSC of a 1-based root port. Thread context.</summary>
    private void WritePortSc(byte port, uint value)
    {
        _registers.Write32(_operationalBase + EhciProtocol.PortScOffset + ((ulong)(port - 1) * EhciProtocol.PortScStride), value);
    }

    /// <summary>PORTSC as a write must carry it: the RW and RWS bits alone, PED included, so no change bit is cleared and nothing starts by accident. Composed from a value read right before the write.</summary>
    private static uint Neutral(uint portsc)
    {
        return portsc & EhciProtocol.PortPreserveMask;
    }

    /// <summary>
    /// Whether a root port has a companion controller to take a full or
    /// low-speed device (EHCI 1.0 section 2.2.3): with port routing rules
    /// the port's HCSP-PORTROUTE nibble names one unless it is 0xF, else
    /// the first N_CC times N_PCC ports have one. Any context.
    /// </summary>
    /// <param name="port">A 1-based root port.</param>
    private bool HasCompanion(byte port)
    {
        if (_hasPortRoutingRules)
        {
            return port >= 1 && port <= _portRoute.Length && _portRoute[port - 1] != EhciProtocol.PortRouteNoCompanion;
        }

        return port <= _companionCount * _companionPortCount;
    }
}
