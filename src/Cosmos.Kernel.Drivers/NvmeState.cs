// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="NvmeDriver"/> holds for one bound controller, hung
/// off <see cref="DeviceBinding.DriverState"/>: the register window, the
/// admin queue pair and the one I/O queue pair in DMA memory, the seven
/// command slots (each a bounce page and an event), the kit lock, the
/// interrupt outcome and the counters the Storage suite reads. The admin
/// queue is polled and driven only by the probe and the detach hook, in
/// thread context on the kit worker. The I/O path (<see cref="Read"/>,
/// <see cref="Write"/>, <see cref="Flush"/>) is entered by the ring from any
/// thread: a slot is claimed and a command submitted under the
/// <see cref="DeviceLock"/>, which is never held across a wait or a delay;
/// with an interrupt the caller waits on the slot's event and
/// <see cref="OnInterrupt"/> drains the completion queue in interrupt
/// context, without the lock, since a lock holder runs with interrupts
/// disabled and the two never overlap; without one the caller drains under
/// the lock between delays. Every entry goes through the DMA spans with
/// <c>MemoryMarshal</c>, never a pointer.
/// </summary>
public sealed class NvmeState
{
    // --- Constants ---

    /// <summary>How long a command may take, and how long a caller waits for a free slot, before it gives up.</summary>
    internal const uint CommandTimeoutMilliseconds = 5000;

    /// <summary>Pause between two completion queue reads without an interrupt, between two slot tries, and between two admin completion reads.</summary>
    internal const uint PollMicroseconds = 10;

    /// <summary>Pause between two CSTS reads while waiting for the ready bit to follow CC.EN.</summary>
    private const uint ReadyPollMicroseconds = 1000;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>The value of <see cref="MessageIndex"/> when no message interrupt is connected.</summary>
    private const int NoMessage = -1;

    /// <summary>Bits in the low dword of a starting LBA.</summary>
    private const int DwordBits = 32;

    // --- Types ---

    /// <summary>
    /// One in-flight command's bookkeeping: its bounce page, the event its
    /// completion signals, and the flags the waiter and the drain share.
    /// <see cref="InUse"/> and <see cref="Quarantined"/> are read and written
    /// under the controller's lock only; <see cref="Status"/> is written by
    /// the drain before <see cref="Done"/>, whose volatile write orders it,
    /// and read by the waiter after its volatile read of <see cref="Done"/>.
    /// </summary>
    private sealed class IoSlot
    {
        /// <summary>Takes the page and the event the probe allocated. Thread context, from the probe.</summary>
        /// <param name="page">One page of DMA memory the command's data goes through.</param>
        /// <param name="deviceEvent">The event the interrupt handler signals when the command completes.</param>
        public IoSlot(DmaBuffer page, DeviceEvent deviceEvent)
        {
            Page = page;
            Event = deviceEvent;
        }

        /// <summary>The bounce page: PRP1 of every command on this slot.</summary>
        public DmaBuffer Page { get; }

        /// <summary>The event the drain signals from interrupt context and the waiter waits on.</summary>
        public DeviceEvent Event { get; }

        /// <summary>Set by the drain once the completion landed, after <see cref="Status"/>; cleared by the submitter under the lock.</summary>
        public volatile bool Done;

        /// <summary>The completion's status field, shifted past the phase tag; 0 is success.</summary>
        public ushort Status;

        /// <summary>True while a caller owns the slot; under the lock.</summary>
        public bool InUse;

        /// <summary>True once a command on the slot timed out or was abandoned: the slot is never handed out again, since a late completion would DMA into a recycled page. Under the lock.</summary>
        public bool Quarantined;
    }

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly uint _doorbellStride;
    private readonly uint _timeoutUnits;
    private IoSlot[] _slots = [];
    private DmaBuffer? _adminSq;
    private DmaBuffer? _adminCq;
    private int _adminSqTail;
    private int _adminCqHead;
    private int _adminPhase = 1;
    private ushort _adminCommandId;
    private DmaBuffer? _ioSq;
    private DmaBuffer? _ioCq;
    private int _ioSqTail;
    private int _ioCqHead;
    private int _ioPhase = 1;
    private volatile bool _ioQueueReady;
    private DeviceLock? _lock;
    private int _messageIndex = NoMessage;
    private int _index;
    private int _namespaceCount;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile int _commandsSent;
    private volatile int _interruptCount;
    private volatile int _timeouts;

    // --- Constructor ---

    /// <summary>
    /// Takes the binding, the BAR0 window and the two CAP fields the probe
    /// snapshotted; the queues, the slots and the lock come later through
    /// the internal setters as the probe brings the controller up. Thread
    /// context, from the probe.
    /// </summary>
    /// <param name="binding">The function's binding, for the log, the delays and the waits.</param>
    /// <param name="registers">The BAR0 window: the registers and the doorbells.</param>
    /// <param name="doorbellStride">CAP.DSTRD: the doorbell stride is 4 bytes shifted left by it.</param>
    /// <param name="timeoutUnits">CAP.TO: the ready transition budget in 500 ms units.</param>
    internal NvmeState(DeviceBinding binding, RegisterWindow registers, uint doorbellStride, uint timeoutUnits)
    {
        _binding = binding;
        _registers = registers;
        _doorbellStride = doorbellStride;
        _timeoutUnits = timeoutUnits;
    }

    // --- Properties the suite reads ---

    /// <summary>The controller's number, the <c>nvme{Index}</c> part of its namespaces' names; assigned when the probe binds. Any context.</summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>True when a message interrupt is connected to <see cref="OnInterrupt"/>, so a command waits on its slot's event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when no message interrupt could be connected, so a command drains the completion queue itself. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>How many namespaces the probe published. Any context.</summary>
    public int NamespaceCount
    {
        get => _namespaceCount;
        internal set => _namespaceCount = value;
    }

    /// <summary>How many I/O commands were put on the I/O submission queue. Any context.</summary>
    public int CommandsSent => _commandsSent;

    /// <summary>How many times the message interrupt ran <see cref="OnInterrupt"/> with the I/O queue created. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many I/O commands went unanswered for <see cref="CommandTimeoutMilliseconds"/>. Any context.</summary>
    public int Timeouts => _timeouts;

    /// <summary>CAP.DSTRD as the probe read it: the doorbell stride is 4 bytes shifted left by this value. Any context.</summary>
    public uint DoorbellStride => _doorbellStride;

    // --- Internal properties the probe sets ---

    /// <summary>The lock every slot claim, release and submission runs under; set by the probe before the I/O queue exists.</summary>
    internal DeviceLock? Lock
    {
        get => _lock;
        set => _lock = value;
    }

    /// <summary>The message the I/O completion queue raises, or -1 when the driver polls.</summary>
    internal int MessageIndex
    {
        get => _messageIndex;
        set => _messageIndex = value;
    }

    /// <summary>The handler's gate: set by the probe once both I/O queues exist, so a message raised before then finds nothing to do.</summary>
    internal bool IoQueueReady
    {
        get => _ioQueueReady;
        set => _ioQueueReady = value;
    }

    // --- Internal methods for the probe and the detach hook ---

    /// <summary>Records the admin queue pair: tail 0, head 0, expected phase 1 (the pages are zeroed, so an entry whose phase bit is 0 is empty). Thread context, from the probe, before the controller is enabled.</summary>
    /// <param name="submissionQueue">One page for the admin submission queue.</param>
    /// <param name="completionQueue">One page for the admin completion queue.</param>
    internal void SetAdminQueues(DmaBuffer submissionQueue, DmaBuffer completionQueue)
    {
        _adminSq = submissionQueue;
        _adminCq = completionQueue;
        _adminSqTail = 0;
        _adminCqHead = 0;
        _adminPhase = 1;
    }

    /// <summary>Records the I/O queue pair: tail 0, head 0, expected phase 1. Thread context, from the probe, before the queues are created on the controller.</summary>
    /// <param name="submissionQueue">One page for the I/O submission queue.</param>
    /// <param name="completionQueue">One page for the I/O completion queue.</param>
    internal void SetIoQueues(DmaBuffer submissionQueue, DmaBuffer completionQueue)
    {
        _ioSq = submissionQueue;
        _ioCq = completionQueue;
        _ioSqTail = 0;
        _ioCqHead = 0;
        _ioPhase = 1;
    }

    /// <summary>Records the command slots, one per page and event. Thread context, from the probe, before the I/O queue exists.</summary>
    /// <param name="pages">One bounce page per slot.</param>
    /// <param name="events">One event per slot, in the same order.</param>
    internal void SetSlots(DmaBuffer[] pages, DeviceEvent[] events)
    {
        IoSlot[] slots = new IoSlot[pages.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new IoSlot(pages[i], events[i]);
        }

        _slots = slots;
    }

    /// <summary>
    /// Clears CC.EN when it is set and waits for CSTS.RDY to clear within
    /// the CAP.TO budget. Thread context on the kit worker, from the probe
    /// before the admin queues are programmed and from the detach hook.
    /// </summary>
    /// <returns>False when the controller stayed ready past the budget.</returns>
    internal bool Disable()
    {
        uint cc = _registers.Read32(NvmeProtocol.Cc);
        if ((cc & NvmeProtocol.CcEnable) != 0)
        {
            _registers.Write32(NvmeProtocol.Cc, cc & ~NvmeProtocol.CcEnable);
        }

        return WaitForReady(ready: false);
    }

    /// <summary>
    /// Polls CSTS.RDY until it reads <paramref name="ready"/>, pausing
    /// <see cref="ReadyPollMicroseconds"/> between reads, within
    /// <c>max(CAP.TO, 1)</c> units of 500 ms. Thread context on the kit worker.
    /// </summary>
    /// <param name="ready">The value waited for.</param>
    /// <returns>False when the budget passed first.</returns>
    internal bool WaitForReady(bool ready)
    {
        uint units = _timeoutUnits == 0 ? 1 : _timeoutUnits;
        long deadline = DeadlineAfter(units * NvmeProtocol.TimeoutUnitMilliseconds);
        while (true)
        {
            bool isReady = (_registers.Read32(NvmeProtocol.Csts) & NvmeProtocol.CstsReady) != 0;
            if (isReady == ready)
            {
                return true;
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return false;
            }

            _binding.Delay(ReadyPollMicroseconds);
        }
    }

    /// <summary>
    /// Submits one admin command and polls the admin completion queue for
    /// its completion: the entry is written whole at the admin tail, the
    /// tail advanced, the stores ordered and the doorbell written; then the
    /// completion queue is read at its head, with the admin doorbell written
    /// after each entry consumed, until the entry with this command's id
    /// arrives (an entry with another id is consumed and ignored) or
    /// <see cref="CommandTimeoutMilliseconds"/> passed. Thread context on the
    /// kit worker, from the probe; the admin queue is always polled.
    /// </summary>
    /// <param name="name">The command's name, for the failure text.</param>
    /// <param name="opcode">The admin opcode.</param>
    /// <param name="nsid">The namespace id, or 0.</param>
    /// <param name="prp1">The physical address of the data page, or 0.</param>
    /// <param name="cdw10">Command dword 10.</param>
    /// <param name="cdw11">Command dword 11.</param>
    /// <param name="status">The completion's status field, shifted past the phase tag; 0 is success. 0 when the command did not complete.</param>
    /// <returns>Null when the command completed; the probe's failure text, <c>{name} did not complete</c>, when it timed out.</returns>
    internal string? SubmitAdmin(string name, byte opcode, uint nsid, ulong prp1, uint cdw10, uint cdw11, out ushort status)
    {
        status = 0;
        DmaBuffer? submissionQueue = _adminSq;
        DmaBuffer? completionQueue = _adminCq;
        if (submissionQueue is null || completionQueue is null)
        {
            return $"{name} did not complete";
        }

        ushort commandId = _adminCommandId++;
        Span<NvmeSqe> entries = MemoryMarshal.Cast<byte, NvmeSqe>(submissionQueue.Span);
        entries[_adminSqTail] = new NvmeSqe
        {
            Cdw0 = opcode | ((uint)commandId << NvmeProtocol.CommandIdShift),
            Nsid = nsid,
            Prp1 = prp1,
            Cdw10 = cdw10,
            Cdw11 = cdw11,
        };
        _adminSqTail = (_adminSqTail + 1) % NvmeProtocol.QueueDepth;
        DmaBuffer.WriteBarrier();
        _registers.Write32(SubmissionDoorbell(0), (uint)_adminSqTail);

        long deadline = DeadlineAfter(CommandTimeoutMilliseconds);
        while (true)
        {
            Span<NvmeCqe> completions = MemoryMarshal.Cast<byte, NvmeCqe>(completionQueue.Span);
            ref NvmeCqe entry = ref completions[_adminCqHead];
            ushort field = Volatile.Read(ref entry.Status);
            if ((field & NvmeProtocol.PhaseTagMask) == _adminPhase)
            {
                DmaBuffer.ReadBarrier();
                ushort completedId = entry.CommandId;
                _adminCqHead++;
                if (_adminCqHead == NvmeProtocol.QueueDepth)
                {
                    _adminCqHead = 0;
                    _adminPhase = 1 - _adminPhase;
                }

                _registers.Write32(CompletionDoorbell(0), (uint)_adminCqHead);
                if (completedId == commandId)
                {
                    status = (ushort)(field >> NvmeProtocol.StatusFieldShift);
                    return null;
                }

                continue;
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                return $"{name} did not complete";
            }

            _binding.Delay(PollMicroseconds);
        }
    }

    /// <summary>Writes one line to the kit log under the binding's node and driver. Thread context; any thread.</summary>
    /// <param name="message">The message.</param>
    internal void Log(string message) => _binding.Log(message);

    // --- The I/O path ---

    /// <summary>
    /// Reads one logical block: claims a slot, submits a Read of NLB 0
    /// into the slot's page, waits, and on success copies
    /// <paramref name="destination"/>'s length of bytes out of the page.
    /// Thread context; any thread.
    /// </summary>
    /// <param name="nsid">The namespace.</param>
    /// <param name="lba">The block.</param>
    /// <param name="destination">Where the block goes; one block long.</param>
    /// <returns>The completion's status field; 0 is success and the block was copied.</returns>
    /// <exception cref="IOException">The command timed out or the binding is being torn down; the slot is quarantined.</exception>
    /// <exception cref="InvalidOperationException">No slot came free within <see cref="CommandTimeoutMilliseconds"/>, or the probe has not created the queues.</exception>
    internal ushort Read(uint nsid, ulong lba, Span<byte> destination)
    {
        int index = AcquireSlot();
        IoSlot slot = _slots[index];
        bool completed = false;
        try
        {
            ushort status = SubmitAndWait(slot, index, NvmeProtocol.IoRead, nsid, lba, hasData: true);
            if (status == 0)
            {
                slot.Page.Span.Slice(0, destination.Length).CopyTo(destination);
            }

            completed = true;
            return status;
        }
        finally
        {
            if (completed)
            {
                ReleaseSlot(index);
            }
            else
            {
                QuarantineSlot(index);
            }
        }
    }

    /// <summary>
    /// Writes one logical block: claims a slot, copies
    /// <paramref name="source"/> into the slot's page and zeroes the rest
    /// of the page (so a block shorter than the page never carries an
    /// earlier command's residue), submits a Write of NLB 0 and waits.
    /// Thread context; any thread.
    /// </summary>
    /// <param name="nsid">The namespace.</param>
    /// <param name="lba">The block.</param>
    /// <param name="source">The block's bytes; one block long.</param>
    /// <returns>The completion's status field; 0 is success.</returns>
    /// <exception cref="IOException">The command timed out or the binding is being torn down; the slot is quarantined.</exception>
    /// <exception cref="InvalidOperationException">No slot came free within <see cref="CommandTimeoutMilliseconds"/>, or the probe has not created the queues.</exception>
    internal ushort Write(uint nsid, ulong lba, ReadOnlySpan<byte> source)
    {
        int index = AcquireSlot();
        IoSlot slot = _slots[index];
        bool completed = false;
        try
        {
            Span<byte> page = slot.Page.Span;
            source.CopyTo(page);
            page.Slice(source.Length).Clear();
            ushort status = SubmitAndWait(slot, index, NvmeProtocol.IoWrite, nsid, lba, hasData: true);
            completed = true;
            return status;
        }
        finally
        {
            if (completed)
            {
                ReleaseSlot(index);
            }
            else
            {
                QuarantineSlot(index);
            }
        }
    }

    /// <summary>Sends a Flush for the namespace on a claimed slot and waits. Thread context; any thread.</summary>
    /// <param name="nsid">The namespace.</param>
    /// <returns>The completion's status field; 0 is success.</returns>
    /// <exception cref="IOException">The command timed out or the binding is being torn down; the slot is quarantined.</exception>
    /// <exception cref="InvalidOperationException">No slot came free within <see cref="CommandTimeoutMilliseconds"/>, or the probe has not created the queues.</exception>
    internal ushort Flush(uint nsid)
    {
        int index = AcquireSlot();
        IoSlot slot = _slots[index];
        bool completed = false;
        try
        {
            ushort status = SubmitAndWait(slot, index, NvmeProtocol.IoFlush, nsid, 0, hasData: false);
            completed = true;
            return status;
        }
        finally
        {
            if (completed)
            {
                ReleaseSlot(index);
            }
            else
            {
                QuarantineSlot(index);
            }
        }
    }

    /// <summary>
    /// The message interrupt's handler: returns at once until the probe
    /// created the I/O queue, counts the interrupt, then drains the I/O
    /// completion queue, signalling each completed slot's event. Interrupt
    /// context; allocation-free; reaches only the DMA spans, the doorbell
    /// and the context. Never runs concurrently with the polled drain: the
    /// lock the latter holds disables interrupts.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        if (!_ioQueueReady)
        {
            return;
        }

        _interruptCount++;
        DrainCompletions(context);
    }

    // --- Private methods ---

    /// <summary>
    /// Submits one I/O command on a claimed slot and waits for its
    /// completion. Under the lock: the slot's status and done flag are
    /// reset, the entry written whole at the I/O tail with the slot index
    /// as its command id, the tail advanced, the stores ordered and the
    /// doorbell written. Then, without the lock, against a deadline of
    /// <see cref="CommandTimeoutMilliseconds"/> from the doorbell write:
    /// with an interrupt the caller waits on the slot's event and re-checks
    /// the done flag after every wake; without one it drains the completion
    /// queue under the lock between delays. A binding being torn down ends
    /// the wait with the detached exception; the deadline with the timeout
    /// exception; both quarantine the slot. Thread context; any thread.
    /// </summary>
    private ushort SubmitAndWait(IoSlot slot, int index, byte opcode, uint nsid, ulong lba, bool hasData)
    {
        DeviceLock deviceLock = RequireLock();
        DmaBuffer submissionQueue = _ioSq ?? throw new InvalidOperationException("The I/O queue is not created.");
        long deadline;
        using (deviceLock.Acquire())
        {
            slot.Status = 0;
            slot.Done = false;
            Span<NvmeSqe> entries = MemoryMarshal.Cast<byte, NvmeSqe>(submissionQueue.Span);
            entries[_ioSqTail] = new NvmeSqe
            {
                Cdw0 = opcode | ((uint)index << NvmeProtocol.CommandIdShift),
                Nsid = nsid,
                Prp1 = hasData ? slot.Page.PhysicalAddress : 0,
                Cdw10 = hasData ? (uint)lba : 0,
                Cdw11 = hasData ? (uint)(lba >> DwordBits) : 0,
                Cdw12 = 0,
            };
            _ioSqTail = (_ioSqTail + 1) % NvmeProtocol.QueueDepth;
            DmaBuffer.WriteBarrier();
            _registers.Write32(SubmissionDoorbell(NvmeProtocol.IoQueueId), (uint)_ioSqTail);
            deadline = DeadlineAfter(CommandTimeoutMilliseconds);
        }

        _commandsSent++;

        if (_hasInterrupt)
        {
            while (true)
            {
                if (slot.Done)
                {
                    return slot.Status;
                }

                if (_binding.IsDetaching)
                {
                    QuarantineSlot(index);
                    throw new IOException("NVMe device detached");
                }

                long now = Stopwatch.GetTimestamp();
                if (now >= deadline)
                {
                    break;
                }

                long remaining = (deadline - now) * MillisecondsPerSecond / Stopwatch.Frequency;
                _binding.Wait(slot.Event, (uint)Math.Max(1, remaining));
            }
        }
        else
        {
            while (true)
            {
                if (slot.Done)
                {
                    return slot.Status;
                }

                if (_binding.IsDetaching)
                {
                    QuarantineSlot(index);
                    throw new IOException("NVMe device detached");
                }

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    break;
                }

                using (deviceLock.Acquire())
                {
                    DrainCompletions(null);
                }

                _binding.Delay(PollMicroseconds);
            }
        }

        _timeouts++;
        QuarantineSlot(index);
        throw new IOException("NVMe command timeout");
    }

    /// <summary>
    /// Drains the I/O completion queue: while the entry at the head carries
    /// the expected phase, orders the loads, records the status on the slot
    /// the command id names (an id past the slots is consumed and ignored),
    /// marks it done, signals its event when a context is given, and
    /// advances the head, flipping the phase on wrap. Writes the completion
    /// doorbell once when anything was drained. Interrupt context from the
    /// handler (no lock), thread context under the lock from the polled
    /// wait; allocation-free.
    /// </summary>
    /// <param name="context">The handler's context, to signal the events; null on the polled path.</param>
    /// <returns>True when at least one entry was consumed.</returns>
    private bool DrainCompletions(InterruptContext? context)
    {
        DmaBuffer? completionQueue = _ioCq;
        if (completionQueue is null)
        {
            return false;
        }

        Span<NvmeCqe> completions = MemoryMarshal.Cast<byte, NvmeCqe>(completionQueue.Span);
        bool drained = false;
        while (true)
        {
            ref NvmeCqe entry = ref completions[_ioCqHead];
            ushort field = Volatile.Read(ref entry.Status);
            if ((field & NvmeProtocol.PhaseTagMask) != _ioPhase)
            {
                break;
            }

            DmaBuffer.ReadBarrier();
            ushort commandId = entry.CommandId;
            if (commandId < _slots.Length)
            {
                IoSlot slot = _slots[commandId];
                slot.Status = (ushort)(field >> NvmeProtocol.StatusFieldShift);
                slot.Done = true;
                context?.Signal(slot.Event);
            }

            _ioCqHead++;
            if (_ioCqHead == NvmeProtocol.QueueDepth)
            {
                _ioCqHead = 0;
                _ioPhase = 1 - _ioPhase;
            }

            drained = true;
        }

        if (drained)
        {
            _registers.Write32(CompletionDoorbell(NvmeProtocol.IoQueueId), (uint)_ioCqHead);
        }

        return drained;
    }

    /// <summary>
    /// Claims the first slot that is neither in use nor quarantined, under
    /// the lock; when none is free, releases the lock, pauses and tries
    /// again until <see cref="CommandTimeoutMilliseconds"/> elapsed. Thread
    /// context; any thread.
    /// </summary>
    /// <returns>The slot's index.</returns>
    /// <exception cref="InvalidOperationException">No slot came free in time.</exception>
    private int AcquireSlot()
    {
        DeviceLock deviceLock = RequireLock();
        long deadline = DeadlineAfter(CommandTimeoutMilliseconds);
        while (true)
        {
            using (deviceLock.Acquire())
            {
                for (int i = 0; i < _slots.Length; i++)
                {
                    IoSlot slot = _slots[i];
                    if (!slot.InUse && !slot.Quarantined)
                    {
                        slot.InUse = true;
                        return i;
                    }
                }
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                throw new InvalidOperationException("NVMe I/O slots exhausted.");
            }

            _binding.Delay(PollMicroseconds);
        }
    }

    /// <summary>Hands a slot back, under the lock. Thread context; any thread.</summary>
    private void ReleaseSlot(int index)
    {
        using (RequireLock().Acquire())
        {
            _slots[index].InUse = false;
        }
    }

    /// <summary>
    /// Takes a slot out of circulation for good, under the lock, leaving it
    /// in use: a command may still be outstanding on it and a late
    /// completion would otherwise land in a recycled page. Logs once per
    /// slot, outside the lock. Thread context; any thread.
    /// </summary>
    private void QuarantineSlot(int index)
    {
        IoSlot slot = _slots[index];
        bool first;
        using (RequireLock().Acquire())
        {
            first = !slot.Quarantined;
            slot.Quarantined = true;
            slot.InUse = true;
        }

        if (first)
        {
            _binding.Log($"quarantined I/O slot {index} (command may still be outstanding)");
        }
    }

    /// <summary>The lock, once the probe created it. Any context.</summary>
    /// <exception cref="InvalidOperationException">The probe has not created the lock.</exception>
    private DeviceLock RequireLock() =>
        _lock ?? throw new InvalidOperationException("The controller's lock is not created.");

    /// <summary>The submission tail doorbell of a queue: <c>0x1000 + (2 * queue) * (4 &lt;&lt; DSTRD)</c>. Any context.</summary>
    private ulong SubmissionDoorbell(int queue) =>
        NvmeProtocol.DoorbellBase + (NvmeProtocol.DoorbellsPerQueue * (ulong)queue) * (NvmeProtocol.DoorbellStrideUnit << (int)_doorbellStride);

    /// <summary>The completion head doorbell of a queue: <c>0x1000 + (2 * queue + 1) * (4 &lt;&lt; DSTRD)</c>. Any context.</summary>
    private ulong CompletionDoorbell(int queue) =>
        NvmeProtocol.DoorbellBase + (NvmeProtocol.DoorbellsPerQueue * (ulong)queue + 1) * (NvmeProtocol.DoorbellStrideUnit << (int)_doorbellStride);

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    private static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;
}
