// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Everything <see cref="OhciDriver"/> holds for one bound controller, hung
/// off <see cref="DeviceBinding.DriverState"/>, and the kit's
/// <see cref="UsbHostController"/> for it: the register window, the HCCA,
/// the permanent heads of the control, bulk and interrupt lists, the
/// descriptor pool, the device table, the pooled device and pipe memory
/// sets, the kit lock, the interrupt outcome and the counters the suites
/// read. Control and bulk transfers are synchronous and run in thread
/// context; completions arrive on the done queue the controller writes
/// into the HCCA, drained by the line interrupt handler (no lock: a holder
/// of the <see cref="DeviceLock"/> runs with interrupts disabled, so the
/// two never overlap) or, on a polled controller and while the host
/// binding detaches, by the waiting thread and the hot-plug thread under
/// the lock. The lock guards every list change, every descriptor the
/// controller may be reading, the device table, the pipe tables and the
/// pools, and is never held across a wait, a delay, a sleep or a kit call
/// other than a report delivery on a polled controller. The busy flags (a
/// device's <see cref="OhciDevice.ControlBusy"/>, a bulk pipe's
/// <see cref="OhciBulkPipe.Busy"/>, the controller's address 0 claim) are
/// claimed under the lock in a delay loop bounded by the operation's own
/// timeout. The partials hold the devices, the control transfers, the bulk
/// transfers, the interrupt pipes, the lists, the events and the ports.
/// </summary>
internal sealed partial class OhciState : UsbHostController
{
    // --- Constants ---

    /// <summary>Bytes of one page: the HCCA, every pool page and every buffer page is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>The probe's failure text when no memory below 4 GiB is left: every OHCI pointer is 32 bits wide.</summary>
    private const string NoLowMemoryFailure = "no DMA memory below 4 GiB";

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly byte _revision;
    private readonly byte _portCount;
    private readonly bool _noPowerSwitching;
    private readonly bool _perPortPowerSwitching;
    private readonly uint _powerOnToPowerGoodMs;
    private readonly OhciDevice?[] _devices = new OhciDevice?[OhciProtocol.MaxAddress + 1];
    private readonly bool[] _addressInUse = new bool[OhciProtocol.MaxAddress + 1];
    private readonly List<OhciDeviceMemory> _freeDeviceSets = [];
    private readonly List<OhciPipeMemory> _freeInterruptPipeMemory = [];
    private readonly List<OhciPipeMemory> _freeBulkPipeMemory = [];
    private readonly List<OhciDescriptorSlot> _retiredSlots = [];
    private readonly OhciDescriptorPool _pool = new();
    private readonly OhciEndpointSlot?[] _intervalHeads = new OhciEndpointSlot?[OhciProtocol.IntervalHeadCount];
    private readonly OhciDevice?[] _recoveryDevices = new OhciDevice?[OhciProtocol.RecoveryBatch];
    private readonly OhciInterruptPipe?[] _recoveryPipes = new OhciInterruptPipe?[OhciProtocol.RecoveryBatch];
    private readonly DeviceEvent _portChangeEvent;
    private readonly WorkItem _faultReport;
    private DmaBuffer? _hcca;
    private OhciEndpointSlot? _controlHead;
    private OhciEndpointSlot? _bulkHead;
    private DeviceLock? _lock;
    private UsbBus? _bus;
    private uint _frameInterval;
    private uint _remoteWakeupConnected;
    private volatile bool _running;
    private volatile bool _rootPortsProbed;
    private volatile bool _recoveryRequested;

    // Address 0: one device at a time answers there, claimed for the whole
    // of an address path under the lock.
    private bool _addressZeroBusy;

    // What the drain saw and could not log: the fault report work item logs
    // it in thread context.
    private volatile bool _transferFaultPending;
    private byte _transferFaultAddress;
    private byte _transferFaultEndpoint;
    private OhciConditionCode _transferFaultCode;
    private volatile bool _pipeStoppedPending;
    private byte _pipeStoppedAddress;
    private byte _pipeStoppedEndpoint;
    private volatile bool _hostEventPending;

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
    private volatile int _staleDoneEntries;
    private volatile int _forgottenDoneEntries;
    private volatile int _strayDoneEntries;
    private volatile int _quarantinedSlots;

    // --- Constructor ---

    /// <summary>
    /// Reads HcRevision and HcRhDescriptorA (the port count, the power
    /// switching mode and the power-good time) and creates the port change
    /// event and the fault report work item. The HCCA, the list heads and
    /// the pool come through <see cref="AllocateStructures"/> and the lock
    /// through <see cref="Lock"/>. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The function's binding, for the DMA memory, the events, the delays, the waits and the log.</param>
    /// <param name="registers">The BAR0 window.</param>
    internal OhciState(DeviceBinding binding, RegisterWindow registers)
    {
        _binding = binding;
        _registers = registers;

        _revision = (byte)(registers.Read32(OhciProtocol.HcRevision) & OhciProtocol.RevisionMask);

        uint descriptorA = registers.Read32(OhciProtocol.HcRhDescriptorA);
        _portCount = (byte)(descriptorA & OhciProtocol.PortCountMask);
        _noPowerSwitching = (descriptorA & OhciProtocol.NoPowerSwitching) != 0;
        _perPortPowerSwitching = (descriptorA & OhciProtocol.PowerSwitchingMode) != 0;
        _powerOnToPowerGoodMs = (descriptorA >> OhciProtocol.PowerOnToPowerGoodShift) * OhciProtocol.PowerOnToPowerGoodUnitMs;

        _portChangeEvent = binding.CreateEvent();
        _faultReport = binding.CreateWorkItem(ReportFaults);
    }

    // --- Properties the suites read ---

    /// <inheritdoc/>
    public override string Name => "OHCI";

    /// <summary>The controller's number, 0 for the first one bound; assigned when the probe binds. Any context.</summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>HcRevision bits 7:0, 0x10 for a 1.0 controller. Any context.</summary>
    public byte Revision => _revision;

    /// <summary>HcRhDescriptorA.NDP: root hub ports. Any context.</summary>
    public byte PortCount => _portCount;

    /// <summary>True when the legacy line interrupt is connected to <see cref="OnInterrupt"/>, so a wait parks on an event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when no line interrupt could be connected, so the waiting thread and the hot-plug thread drain the done queue. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True once the <c>ohci-hotplug</c> thread started; false without a scheduler, when only boot-time devices are enumerated. Any context.</summary>
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

    /// <summary>How many control and bulk transfers went unanswered for their budget. Any context.</summary>
    public int Timeouts => _timeouts;

    /// <summary>How many devices the address path gave an address and a descriptor. Any context.</summary>
    public int DevicesAddressed => _devicesAddressed;

    /// <summary>How many pipes were closed, by a driver or by a binding's unwind. Any context.</summary>
    public int PipesClosed => _pipesClosed;

    /// <summary>How many times the line interrupt ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many interrupt pipe recoveries were started after a transfer error. Any context.</summary>
    public int PipeRecoveries => _pipeRecoveries;

    /// <summary>How many interrupt pipes were stopped for good after errors nobody could recover. Any context.</summary>
    public int PipesStopped => _pipesStopped;

    /// <summary>How many unrecoverable errors the controller raised. Any context.</summary>
    public int HostControllerEvents => _hostControllerEvents;

    /// <summary>How many done queue entries named a TD nobody waited for any more. Any context.</summary>
    public int StaleDoneEntries => _staleDoneEntries;

    /// <summary>How many TDs the controller retired without a done queue entry, taken back when a later TD of the same ED was delivered. Any context.</summary>
    public int ForgottenDoneEntries => _forgottenDoneEntries;

    /// <summary>How many done queue walks stopped at an address outside the pool, or after more entries than the pool holds slots. Any context.</summary>
    public int StrayDoneEntries => _strayDoneEntries;

    /// <summary>How many TD slots were parked for good because the controller never delivered them. Any context.</summary>
    public int QuarantinedSlots => _quarantinedSlots;

    // --- Internal properties the probe reads and sets ---

    /// <summary>The lock every list change, descriptor write and table change runs under; set by the probe before the handler is connected.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the lock.</exception>
    internal DeviceLock Lock
    {
        get => _lock ?? throw new InvalidOperationException("The controller's lock is not created.");
        set => _lock = value;
    }

    /// <summary>Bytes from BAR0 the driver touches: the last port register.</summary>
    internal ulong RequiredRegisterLength => OhciProtocol.HcRhPortStatusBase + (OhciProtocol.PortRegisterStride * _portCount);

    /// <summary>True from the controller's start to its stop, or to the unrecoverable error that stopped it. Any context.</summary>
    internal bool Running
    {
        get => _running;
        set => _running = value;
    }

    /// <summary>
    /// <see cref="Running"/> and HCFS operational: a controller that
    /// stopped itself runs no list, and a PCI function that vanished reads
    /// all ones, which is HCFS suspend, so a pipe close or a quiesce
    /// against either drops its descriptors instead of waiting for the
    /// controller to let go. Any context.
    /// </summary>
    private bool ControllerRuns
    {
        get
        {
            return _running && (Read(OhciProtocol.HcControl) & OhciProtocol.FunctionalStateMask) == OhciProtocol.FunctionalStateOperational;
        }
    }

    /// <summary>The permanent head of the control list, once allocated.</summary>
    private OhciEndpointSlot ControlHead => _controlHead ?? throw new InvalidOperationException("The control list head is not allocated.");

    /// <summary>The permanent head of the bulk list, once allocated.</summary>
    private OhciEndpointSlot BulkHead => _bulkHead ?? throw new InvalidOperationException("The bulk list head is not allocated.");

    // --- Allocation ---

    /// <summary>
    /// The one allocation policy: every OHCI pointer is 32 bits wide, so
    /// memory is taken below 4 GiB only and null comes back when there is
    /// none. Thread context.
    /// </summary>
    /// <param name="length">Bytes wanted.</param>
    /// <param name="alignment">Alignment of the first byte.</param>
    /// <returns>The buffer, or null when no memory below 4 GiB was found.</returns>
    /// <exception cref="InvalidOperationException">No pages left, or the binding is being torn down.</exception>
    internal DmaBuffer? AllocateDma(int length, int alignment)
    {
        return _binding.TryAllocateDma(length, alignment, DmaConstraints.Addressable32Bit, out DmaBuffer? buffer) ? buffer : null;
    }

    /// <summary><see cref="AllocateDma"/> for the pools, where exhaustion or a detaching binding is a null open rather than an exception. Thread context.</summary>
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

    /// <summary>
    /// Allocates and builds in memory what the controller is programmed
    /// with: the HCCA page, the pool's first page, the permanent control
    /// and bulk list heads and the six interval heads (each a skipped ED
    /// with no queue), the interval heads chained from the longest period
    /// to the shortest, and the HCCA interrupt table pointing frame i at
    /// the head whose period divides it (OHCI 1.0a sections 4.4 and 5.2.7).
    /// Runs before the reset, so the window between the reset and the
    /// operational state holds register writes only. Thread context, from
    /// the probe, before the lock exists.
    /// </summary>
    /// <returns>Null when everything was allocated; the probe's failure text when no memory below 4 GiB was left.</returns>
    internal string? AllocateStructures()
    {
        DmaBuffer? hcca = AllocateDma(PageBytes, PageBytes);
        DmaBuffer? poolPage = AllocateDma(PageBytes, PageBytes);
        if (hcca is null || poolPage is null)
        {
            return NoLowMemoryFailure;
        }

        _hcca = hcca;
        _pool.AddPage(new OhciDescriptorPage(poolPage));

        if (!_pool.TryTake(out OhciDescriptorSlot controlSlot) || !_pool.TryTake(out OhciDescriptorSlot bulkSlot))
        {
            return NoLowMemoryFailure;
        }

        _controlHead = new OhciEndpointSlot(controlSlot);
        _bulkHead = new OhciEndpointSlot(bulkSlot);
        WriteSkippedHead(_controlHead, 0);
        WriteSkippedHead(_bulkHead, 0);

        for (int k = 0; k < _intervalHeads.Length; k++)
        {
            if (!_pool.TryTake(out OhciDescriptorSlot slot))
            {
                return NoLowMemoryFailure;
            }

            OhciEndpointSlot head = new(slot);
            OhciEndpointSlot? shorter = k == 0 ? null : _intervalHeads[k - 1];
            head.Next = shorter;
            WriteSkippedHead(head, shorter?.PhysicalAddress ?? 0);
            _intervalHeads[k] = head;
        }

        Span<uint> table = InterruptTable();
        for (int i = 0; i < OhciHcca.InterruptTableEntries; i++)
        {
            int k = i == 0 ? _intervalHeads.Length - 1 : Math.Min(BitOperations.TrailingZeroCount(i), _intervalHeads.Length - 1);
            table[i] = IntervalHead(k).PhysicalAddress;
        }

        DmaBuffer.WriteBarrier();
        return null;
    }

    /// <summary>Writes a permanent head ED: skipped, no queue, linked to <paramref name="next"/>. Thread context, from the probe.</summary>
    private static void WriteSkippedHead(OhciEndpointSlot head, uint next)
    {
        ref OhciEndpointDescriptor ed = ref Ed(head.Descriptor);
        ed.Control = OhciProtocol.EndpointSkip;
        ed.TailPointer = 0;
        ed.HeadPointer = 0;
        ed.NextEndpoint = next;
    }

    /// <summary>The interval head of period 2^<paramref name="exponent"/> frames, once allocated.</summary>
    private OhciEndpointSlot IntervalHead(int exponent)
    {
        return _intervalHeads[exponent] ?? throw new InvalidOperationException("The interrupt tree is not allocated.");
    }

    /// <summary>
    /// Takes a free pool slot, adding a page when the pool ran dry. Thread
    /// context; the pool is read and written under the lock, the page
    /// allocated outside it.
    /// </summary>
    /// <returns>The slot, or <see cref="OhciDescriptorSlot.None"/> when no memory below 4 GiB was left.</returns>
    private OhciDescriptorSlot TakeDescriptorSlot()
    {
        using (Lock.Acquire())
        {
            if (_pool.TryTake(out OhciDescriptorSlot slot))
            {
                return slot;
            }
        }

        DmaBuffer? buffer = TryAllocatePooled(PageBytes, PageBytes);
        if (buffer is null)
        {
            return OhciDescriptorSlot.None;
        }

        OhciDescriptorPage page = new(buffer);
        using (Lock.Acquire())
        {
            _pool.AddPage(page);
            return _pool.TryTake(out OhciDescriptorSlot slot) ? slot : OhciDescriptorSlot.None;
        }
    }

    /// <summary>Takes <paramref name="count"/> pool slots, giving back the ones taken when one cannot be had. Thread context.</summary>
    /// <returns>The slots, or null when no memory below 4 GiB was left.</returns>
    private OhciDescriptorSlot[]? TakeDescriptorSlots(int count)
    {
        OhciDescriptorSlot[] slots = new OhciDescriptorSlot[count];
        for (int i = 0; i < count; i++)
        {
            slots[i] = TakeDescriptorSlot();
            if (!slots[i].IsNone)
            {
                continue;
            }

            using (Lock.Acquire())
            {
                for (int taken = 0; taken < i; taken++)
                {
                    _pool.Return(slots[taken]);
                }
            }

            return null;
        }

        return slots;
    }

    /// <summary>
    /// Takes a free device memory set or allocates one (the control ED,
    /// four TDs, the SETUP slot, the bounce page and an event), reset. A
    /// pooled set that quarantines left without the slots for a control
    /// transfer is dropped, never pooled again, and a fresh one built in
    /// its place. When the page or the event of a fresh set cannot be had
    /// the slots go back to the pool; the page and the event stay with the
    /// kit until the teardown. Thread context, the address path.
    /// </summary>
    /// <returns>The set, or null when no DMA memory was left or the binding is being torn down.</returns>
    private OhciDeviceMemory? TakeDeviceMemory()
    {
        OhciDeviceMemory? memory = null;
        using (Lock.Acquire())
        {
            int last = _freeDeviceSets.Count - 1;
            if (last >= 0)
            {
                memory = _freeDeviceSets[last];
                _freeDeviceSets.RemoveAt(last);
            }
        }

        if (memory is not null)
        {
            memory.Reset();
            if (memory.IsUsable)
            {
                return memory;
            }
        }

        OhciDescriptorSlot endpoint = TakeDescriptorSlot();
        OhciDescriptorSlot setup = TakeDescriptorSlot();
        OhciDescriptorSlot[]? transfers = TakeDescriptorSlots(OhciProtocol.DeviceTransferSlots);
        DmaBuffer? bounce = TryAllocatePooled(PageBytes, PageBytes);
        DeviceEvent? controlEvent = TryCreateEvent();
        if (endpoint.IsNone || setup.IsNone || transfers is null || bounce is null || controlEvent is null)
        {
            ReturnDescriptorSlots(endpoint, setup, transfers);
            return null;
        }

        memory = new OhciDeviceMemory(endpoint, transfers, setup, bounce, controlEvent);
        memory.Reset();
        return memory;
    }

    /// <summary>
    /// Takes a free pipe memory set of one kind or allocates one, reset. A
    /// pooled set that quarantines left without the slots for a transfer
    /// is dropped, never pooled again, and a fresh one built in its place.
    /// When the buffer or the event of a fresh set cannot be had the slots
    /// go back to the pool; the buffer and the event stay with the kit
    /// until the teardown. Thread context, the open path.
    /// </summary>
    /// <param name="bulk">True for an ED, two TDs, the 64 KiB bounce and an event; false for an ED, nine TDs and one buffer page.</param>
    /// <returns>The set, or null when no DMA memory was left or the binding is being torn down.</returns>
    private OhciPipeMemory? TakePipeMemory(bool bulk)
    {
        List<OhciPipeMemory> pool = bulk ? _freeBulkPipeMemory : _freeInterruptPipeMemory;
        OhciPipeMemory? memory = null;
        using (Lock.Acquire())
        {
            int last = pool.Count - 1;
            if (last >= 0)
            {
                memory = pool[last];
                pool.RemoveAt(last);
            }
        }

        if (memory is not null)
        {
            memory.Reset();
            if (memory.IsUsable)
            {
                return memory;
            }
        }

        OhciDescriptorSlot endpoint = TakeDescriptorSlot();
        OhciDescriptorSlot[]? transfers = TakeDescriptorSlots(bulk ? OhciProtocol.BulkTransferSlots : OhciProtocol.InterruptTransferSlots);
        DmaBuffer? buffer = bulk
            ? TryAllocatePooled(OhciProtocol.BulkBufferBytes, OhciProtocol.BulkBufferBytes)
            : TryAllocatePooled(PageBytes, PageBytes);
        DeviceEvent? completion = bulk ? TryCreateEvent() : null;
        if (endpoint.IsNone || transfers is null || buffer is null || (bulk && completion is null))
        {
            ReturnDescriptorSlots(endpoint, OhciDescriptorSlot.None, transfers);
            return null;
        }

        memory = new OhciPipeMemory(endpoint, transfers, buffer, completion);
        memory.Reset();
        return memory;
    }

    /// <summary>Gives the slots a set allocation took back to the pool when the rest of the set could not be had: the controller never saw them. Thread context.</summary>
    /// <param name="endpoint">The ED slot, or none.</param>
    /// <param name="setup">The SETUP slot of a device set, or none.</param>
    /// <param name="transfers">The TD slots, or null when they could not be taken.</param>
    private void ReturnDescriptorSlots(OhciDescriptorSlot endpoint, OhciDescriptorSlot setup, OhciDescriptorSlot[]? transfers)
    {
        using (Lock.Acquire())
        {
            _pool.Return(endpoint);
            _pool.Return(setup);
            if (transfers is not null)
            {
                foreach (OhciDescriptorSlot slot in transfers)
                {
                    _pool.Return(slot);
                }
            }
        }
    }

    /// <summary>Creates an event, or null once the binding is being torn down (the kit refuses new resources then). Thread context.</summary>
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

    /// <summary>Returns a pipe memory set to its pool. Under the lock.</summary>
    private void ReturnPipeMemory(OhciPipeMemory memory)
    {
        if (memory.IsBulk)
        {
            _freeBulkPipeMemory.Add(memory);
        }
        else
        {
            _freeInterruptPipeMemory.Add(memory);
        }
    }

    // --- Waits ---

    /// <summary>
    /// Waits for a completion flag the handler or the polled drain sets:
    /// true at once when it is set; false when the controller stopped
    /// itself, when the device is disconnected or the host binding
    /// detaches (a transfer wait only), or when the deadline passed. With
    /// an interrupt, and while the host binding is not detaching, the
    /// thread parks on <paramref name="evt"/> for the remaining time and
    /// re-checks the flag after every wake; otherwise it drains the
    /// controller under the lock between delays, which is how the pipe
    /// closes of a teardown complete after the kit cancelled the binding's
    /// events. The lock is never held across the wait. Thread context.
    /// </summary>
    /// <param name="completed">The flag, a plain field written with <c>Volatile</c>.</param>
    /// <param name="evt">The event the completion signals.</param>
    /// <param name="device">The device whose disconnection ends the wait, or null for a controller wait bound by its budget alone.</param>
    /// <param name="timeoutMs">The budget.</param>
    private bool WaitCompletion(ref bool completed, DeviceEvent evt, OhciDevice? device, uint timeoutMs)
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
                using (Lock.Acquire())
                {
                    DrainCompletions(null);
                }

                _binding.Delay(OhciProtocol.PollMicroseconds);
            }
        }
    }

    /// <summary>
    /// Claims a device's control ED under the lock, in a delay loop bounded
    /// by <paramref name="timeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set. Thread context.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="timeoutMs">The budget.</param>
    /// <returns>Success when claimed; Disconnected or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimControl(OhciDevice device, uint timeoutMs)
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

            _binding.Delay(OhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a device's control claim under the lock. Thread context.</summary>
    private void ReleaseControl(OhciDevice device)
    {
        using (Lock.Acquire())
        {
            device.ControlBusy = false;
        }
    }

    /// <summary>
    /// Claims a bulk pipe under the lock, in a delay loop bounded by
    /// <see cref="OhciProtocol.BulkTimeoutMs"/>; every pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming when it is set, and reads
    /// <see cref="UsbPipe.IsClosed"/> in the same lock hold as the claim,
    /// returning Error for a pipe closed meanwhile: a close takes the
    /// claim, pools the set and clears the claim in one locked step, so a
    /// claim landing after it would otherwise run on a pooled set.
    /// Thread context.
    /// </summary>
    /// <param name="device">The pipe's device.</param>
    /// <param name="pipe">The pipe.</param>
    /// <returns>Success when claimed; Disconnected, Error or Timeout otherwise.</returns>
    private UsbTransferStatus ClaimBulk(OhciDevice device, OhciBulkPipe pipe)
    {
        long deadline = DeadlineAfter(OhciProtocol.BulkTimeoutMs);
        while (true)
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            using (Lock.Acquire())
            {
                if (pipe.IsClosed)
                {
                    return UsbTransferStatus.Error;
                }

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

            _binding.Delay(OhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases a bulk pipe's claim under the lock. Thread context.</summary>
    private void ReleaseBulk(OhciBulkPipe pipe)
    {
        using (Lock.Acquire())
        {
            pipe.Busy = false;
        }
    }

    /// <summary>
    /// Claims address 0 for one address path under the lock, in a delay
    /// loop bounded by <see cref="OhciProtocol.AddressClaimTimeoutMs"/>:
    /// only one device may answer there, and the kit may enumerate a root
    /// port and a hub port at the same time. Thread context.
    /// </summary>
    /// <returns>False when the claim stayed held past the budget.</returns>
    private bool ClaimAddressZero()
    {
        long deadline = DeadlineAfter(OhciProtocol.AddressClaimTimeoutMs);
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

            _binding.Delay(OhciProtocol.PollMicroseconds);
        }
    }

    /// <summary>Releases the address 0 claim under the lock. Thread context.</summary>
    private void ReleaseAddressZero()
    {
        using (Lock.Acquire())
        {
            _addressZeroBusy = false;
        }
    }

    /// <summary>
    /// Waits <paramref name="milliseconds"/> in 1 ms delay steps, leaving
    /// early when the host binding begins detaching, so the hot-plug thread
    /// leaves within one step of the teardown flag. For port waits; a
    /// descriptor settle uses <see cref="SettleFrames"/>, which the
    /// teardown must not cut short. Thread context.
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

            _binding.Delay(OhciProtocol.MicrosecondsPerMillisecond);
        }

        return !_binding.IsDetaching;
    }

    /// <summary>
    /// Waits <paramref name="frames"/> milliseconds for the controller to
    /// pass a skipped or unlinked ED: a controller wait, bound by its own
    /// length alone, so a close during the teardown still gives the
    /// controller its frames. Thread context.
    /// </summary>
    /// <param name="frames">Frames to wait, one millisecond each.</param>
    private void SettleFrames(uint frames)
    {
        _binding.Delay(frames * OhciProtocol.MicrosecondsPerMillisecond);
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

    // --- Registers and shared memory ---

    /// <summary>Reads an operational register. Any context.</summary>
    private uint Read(ulong offset)
    {
        return _registers.Read32(offset);
    }

    /// <summary>Writes an operational register. Any context.</summary>
    private void Write(ulong offset, uint value)
    {
        _registers.Write32(offset, value);
    }

    /// <summary>Reads HcRhPortStatus of a 1-based root port. Any context.</summary>
    private uint ReadPortStatus(byte port)
    {
        return _registers.Read32(OhciProtocol.HcRhPortStatusBase + ((ulong)(port - 1) * OhciProtocol.PortRegisterStride));
    }

    /// <summary>Writes HcRhPortStatus of a 1-based root port: only the bits meant, since every bit is a set, clear or W1C command. Thread context.</summary>
    private void WritePortStatus(byte port, uint value)
    {
        _registers.Write32(OhciProtocol.HcRhPortStatusBase + ((ulong)(port - 1) * OhciProtocol.PortRegisterStride), value);
    }

    /// <summary>Changes HCFS alone, every other HcControl bit (RWC among them) carried back as read. Thread context.</summary>
    private void WriteFunctionalState(uint state)
    {
        Write(OhciProtocol.HcControl, (Read(OhciProtocol.HcControl) & ~OhciProtocol.FunctionalStateMask) | state);
    }

    /// <summary>The HCCA, once allocated. Any context, allocation-free.</summary>
    private ref OhciHcca Hcca()
    {
        DmaBuffer hcca = _hcca ?? throw new InvalidOperationException("The HCCA is not allocated.");
        return ref MemoryMarshal.Cast<byte, OhciHcca>(hcca.Span)[0];
    }

    /// <summary>The HCCA's 32 interrupt list heads. Thread context, from the probe.</summary>
    private Span<uint> InterruptTable()
    {
        DmaBuffer hcca = _hcca ?? throw new InvalidOperationException("The HCCA is not allocated.");
        return MemoryMarshal.Cast<byte, uint>(hcca.Span.Slice(0, OhciHcca.InterruptTableBytes));
    }

    /// <summary>The ED in <paramref name="slot"/>. Any context, allocation-free.</summary>
    private static ref OhciEndpointDescriptor Ed(OhciDescriptorSlot slot)
    {
        OhciDescriptorPage page = slot.Page ?? throw new InvalidOperationException("The endpoint slot is empty.");
        return ref page.Endpoint(slot.Index);
    }

    /// <summary>The TD in <paramref name="slot"/>. Any context, allocation-free.</summary>
    private static ref OhciTransferDescriptor Td(OhciDescriptorSlot slot)
    {
        OhciDescriptorPage page = slot.Page ?? throw new InvalidOperationException("The transfer slot is empty.");
        return ref page.Transfer(slot.Index);
    }

    // --- Internal methods for the probe and the detach hook ---

    /// <summary>
    /// Asks the firmware to hand the controller over when HcControl.IR says
    /// an SMI handler owns it: the OC source enabled (the request reaches
    /// the handler as an SMI through that source alone, and the firmware
    /// may have left it off; Linux ohci_init does the same), OCR set, IR
    /// polled clear for up to
    /// <see cref="OhciProtocol.FirmwareHandoffTimeoutMs"/>, else logged and
    /// cleared with a preserving HcControl write (OHCI 1.0a sections
    /// 5.1.1.3.2 and 7.1.3). Nothing else is written: the interrupt disable
    /// after the reset, which re-arms the master enable, takes the OC
    /// enable away again. Thread context, from the probe.
    /// </summary>
    internal void TakeOwnershipFromFirmware()
    {
        if ((Read(OhciProtocol.HcControl) & OhciProtocol.InterruptRouting) == 0)
        {
            return;
        }

        Write(OhciProtocol.HcInterruptEnable, OhciProtocol.OwnershipChange);
        Write(OhciProtocol.HcCommandStatus, OhciProtocol.OwnershipChangeRequest);
        uint waitedMs = 0;
        while ((Read(OhciProtocol.HcControl) & OhciProtocol.InterruptRouting) != 0 && waitedMs < OhciProtocol.FirmwareHandoffTimeoutMs)
        {
            _binding.Delay(OhciProtocol.MicrosecondsPerMillisecond);
            waitedMs++;
        }

        uint control = Read(OhciProtocol.HcControl);
        if ((control & OhciProtocol.InterruptRouting) != 0)
        {
            _binding.Log("firmware did not release the controller, taking it over");
            Write(OhciProtocol.HcControl, control & (OhciProtocol.RemoteWakeupConnected | OhciProtocol.FunctionalStateMask));
        }
    }

    /// <summary>
    /// Brings the bus to a defined state and resets the controller (OHCI
    /// 1.0a section 5.1.1.3): FI and RWC saved; an operational controller
    /// left alone, a suspended or resuming one resumed and settled, a reset
    /// one held in bus reset and settled; then always the software reset
    /// of <see cref="SoftwareReset"/>. It is never skipped: a controller
    /// the firmware left operational keeps its own HCCA and lists, and
    /// reprogramming under it would retire its TDs into this driver's done
    /// queue. After HCR the controller is suspended and must become
    /// operational within 2 ms, so <see cref="ProgramRegisters"/> follows
    /// at once. Thread context, from the probe.
    /// </summary>
    /// <returns>Null when the reset completed; the probe's failure text otherwise.</returns>
    internal string? Reset()
    {
        uint interval = Read(OhciProtocol.HcFmInterval) & OhciProtocol.FrameIntervalMask;
        _frameInterval = interval == 0 ? OhciProtocol.DefaultFrameInterval : interval;

        uint control = Read(OhciProtocol.HcControl);
        _remoteWakeupConnected = control & OhciProtocol.RemoteWakeupConnected;
        switch (control & OhciProtocol.FunctionalStateMask)
        {
            case OhciProtocol.FunctionalStateOperational:
                break;

            case OhciProtocol.FunctionalStateSuspend:
            case OhciProtocol.FunctionalStateResume:
                WriteFunctionalState(OhciProtocol.FunctionalStateResume);
                _binding.Delay(OhciProtocol.ResumeSettleMs * OhciProtocol.MicrosecondsPerMillisecond);
                break;

            default:
                WriteFunctionalState(OhciProtocol.FunctionalStateReset);
                _binding.Delay(OhciProtocol.BusResetSettleMs * OhciProtocol.MicrosecondsPerMillisecond);
                break;
        }

        return SoftwareReset();
    }

    /// <summary>HCR set and polled clear every 10 us up to <see cref="OhciProtocol.ResetTimeoutMs"/> (OHCI 1.0a section 7.1.3); the controller is suspended afterwards. Thread context, from the probe.</summary>
    /// <returns>Null when the reset completed; the probe's failure text otherwise.</returns>
    private string? SoftwareReset()
    {
        Write(OhciProtocol.HcCommandStatus, OhciProtocol.HostControllerReset);
        uint polls = OhciProtocol.ResetTimeoutMs * OhciProtocol.MicrosecondsPerMillisecond / OhciProtocol.ResetPollMicroseconds;
        for (uint polled = 0; (Read(OhciProtocol.HcCommandStatus) & OhciProtocol.HostControllerReset) != 0; polled++)
        {
            if (polled >= polls)
            {
                return "the controller reset did not complete";
            }

            _binding.Delay(OhciProtocol.ResetPollMicroseconds);
        }

        return null;
    }

    /// <summary>
    /// Programs the reset controller with register writes only: every
    /// interrupt off (the reset re-armed the master enable), the HCCA, the
    /// control and bulk list heads (written here once, never again),
    /// HcFmInterval with FIT toggled and FSMPS derived from the saved FI,
    /// HcPeriodicStart at 90 percent of it, HcLSThreshold, every status
    /// bit cleared; then, once HcFmInterval and HcPeriodicStart read back
    /// as taken, HcControl with the saved RWC, a 4:1 control to bulk
    /// ratio, the three lists enabled and HCFS operational (OHCI 1.0a
    /// section 5.1.1.4). Some controllers (SiS, OPTi) drop the first
    /// HcFmInterval write after a software reset and would run with a
    /// largest packet size of 0, scheduling nothing: FSMPS or
    /// HcPeriodicStart reading 0 is retried once as Linux ohci_run does
    /// with its INITRESET quirk, HCR again and HCFS put to USBReset
    /// before the writes, so HcFmInterval is written outside USBSuspend;
    /// still 0 fails the probe before the operational write, with the
    /// controller held in USBReset and no DMA started, so no stop is
    /// needed. Review-only on QEMU, where the write always takes. Thread
    /// context, from the probe, right after <see cref="Reset"/>.
    /// </summary>
    /// <returns>Null when the controller runs; the probe's failure text when the retry's reset did not complete or the frame interval was not taken.</returns>
    internal string? ProgramRegisters()
    {
        DmaBuffer hcca = _hcca ?? throw new InvalidOperationException("The HCCA is not allocated.");
        WriteProgramRegisters(hcca);
        if (!FrameIntervalTaken())
        {
            string? failure = SoftwareReset();
            if (failure is not null)
            {
                return failure;
            }

            Write(OhciProtocol.HcControl, _remoteWakeupConnected | OhciProtocol.FunctionalStateReset);

            // The read posts the HCFS write before the programming writes follow it.
            Read(OhciProtocol.HcControl);
            WriteProgramRegisters(hcca);
            if (!FrameIntervalTaken())
            {
                return "the controller did not take its frame interval";
            }
        }

        Write(OhciProtocol.HcControl, _remoteWakeupConnected | OhciProtocol.ControlBulkRatio4To1 | OhciProtocol.PeriodicListEnable
            | OhciProtocol.ControlListEnable | OhciProtocol.BulkListEnable | OhciProtocol.FunctionalStateOperational);
        _running = true;
        return null;
    }

    /// <summary>The programming writes of <see cref="ProgramRegisters"/>, HcControl left alone: every interrupt off, the HCCA, the two list heads, HcFmInterval with FIT toggled, HcPeriodicStart, HcLSThreshold and every status bit cleared. Thread context, from the probe.</summary>
    /// <param name="hcca">The HCCA page.</param>
    private void WriteProgramRegisters(DmaBuffer hcca)
    {
        Write(OhciProtocol.HcInterruptDisable, OhciProtocol.AllInterrupts);
        Write(OhciProtocol.HcHcca, (uint)hcca.PhysicalAddress);
        Write(OhciProtocol.HcControlHeadEd, ControlHead.PhysicalAddress);
        Write(OhciProtocol.HcBulkHeadEd, BulkHead.PhysicalAddress);

        uint toggle = (Read(OhciProtocol.HcFmInterval) & OhciProtocol.FrameIntervalToggle) ^ OhciProtocol.FrameIntervalToggle;
        uint largestPacket = (_frameInterval - OhciProtocol.MaxPacketOverheadBits) * OhciProtocol.LargestDataPacketNumerator / OhciProtocol.LargestDataPacketDenominator;
        Write(OhciProtocol.HcFmInterval, toggle | (largestPacket << OhciProtocol.LargestDataPacketShift) | _frameInterval);
        Write(OhciProtocol.HcPeriodicStart, _frameInterval * OhciProtocol.PeriodicStartNumerator / OhciProtocol.PeriodicStartDenominator);
        Write(OhciProtocol.HcLsThreshold, OhciProtocol.DefaultLowSpeedThreshold);
        Write(OhciProtocol.HcInterruptStatus, OhciProtocol.InterruptSourceMask);
    }

    /// <summary>True when HcFmInterval holds a largest data packet size and HcPeriodicStart a start: the programming writes took, which Linux ohci_run checks the same way. Thread context.</summary>
    private bool FrameIntervalTaken()
    {
        return (Read(OhciProtocol.HcFmInterval) & OhciProtocol.LargestDataPacketMask) != 0 && Read(OhciProtocol.HcPeriodicStart) != 0;
    }

    /// <summary>
    /// Enables the done queue, root hub and unrecoverable error sources and
    /// the master enable, once the line interrupt is connected and the boot
    /// scan is done. The scan's own port writes latched RHSC (a port reset
    /// and every change bit written back set it), so that status bit is
    /// written back first, or the enable would raise the line at once for
    /// changes the scan already handled; RHSC alone, never WDH, so a done
    /// head is never dropped. Thread context, from the probe.
    /// </summary>
    internal void EnableInterrupts()
    {
        Write(OhciProtocol.HcInterruptStatus, OhciProtocol.RootHubStatusChange);
        Write(OhciProtocol.HcInterruptEnable, OhciProtocol.EnabledInterrupts);
    }

    /// <summary>
    /// Stops the controller: every interrupt off, the lists disabled and
    /// HCFS back to USBReset with RWC preserved, and <see cref="Running"/>
    /// cleared. Thread context, from the detach hook with the hardware
    /// present or from a probe that failed after the controller started.
    /// </summary>
    internal void Stop()
    {
        Write(OhciProtocol.HcInterruptDisable, OhciProtocol.AllInterrupts);
        Write(OhciProtocol.HcControl, (Read(OhciProtocol.HcControl) & OhciProtocol.RemoteWakeupConnected) | OhciProtocol.FunctionalStateReset);
        _running = false;
    }
}
