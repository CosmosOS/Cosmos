// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The disk <see cref="VirtioBlkDriver"/> publishes: <c>vblk&lt;n&gt;</c>
/// with the geometry the configuration space reported, over one request
/// queue and one DMA bounce block. One request is in flight at a time; a
/// caller finding the block busy waits for it. Every method is thread
/// context, entered by the ring from any thread; a failed request surfaces
/// as an <see cref="IOException"/>, as the contract asks. The
/// <see cref="DeviceLock"/> is taken around the slot claim, the descriptor
/// writes with the submit and the notify, and each used ring take, never
/// across a wait, a delay, a copy or a log; <see cref="OnInterrupt"/> runs
/// in interrupt context and only counts and signals the event.
/// </summary>
public sealed class VirtioBlkState : IBlockDevice
{
    // --- Constants ---

    /// <summary>How long a submitted request may take before the device is declared faulted.</summary>
    internal const uint RequestTimeoutMilliseconds = 5000;

    /// <summary>How long a caller waits for the request slot another caller holds.</summary>
    internal const uint BusyWaitMilliseconds = 5000;

    /// <summary>Pause between two used ring reads when the queue has no interrupt, and between two tries for the request slot.</summary>
    internal const uint PollMicroseconds = 10;

    /// <summary>Descriptors of one request: the header, the data and the status.</summary>
    internal const int DescriptorsPerRequest = 3;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly VirtioAccess _device;
    private readonly Virtqueue _queue;
    private readonly DmaBuffer _block;
    private readonly ulong _blockCount;
    private readonly ulong _blockSize;
    private readonly int _maxTransferBytes;
    private readonly bool _readOnly;
    private readonly bool _flushNegotiated;
    private readonly uint _index;
    private readonly string _name;

    /// <summary>Set under the lock while a caller owns the request slot and the bounce block; left set for good once the device faulted.</summary>
    private bool _busy;

    /// <summary>The in-flight chain's data descriptor, valid when <see cref="_inFlightHasData"/>.</summary>
    private ushort _inFlightData;

    /// <summary>The in-flight chain's status descriptor.</summary>
    private ushort _inFlightStatus;

    /// <summary>True when the in-flight chain has a data descriptor between the header and the status.</summary>
    private bool _inFlightHasData;

    private volatile bool _faulted;
    private volatile bool _faultLogged;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile int _requestsSubmitted;
    private volatile int _requestsCompleted;
    private volatile int _timeouts;
    private volatile int _interruptCount;

    // --- Constructor ---

    /// <summary>
    /// Takes the access, the queue, the bounce block and the geometry the
    /// probe acquired; the lock and the event come right after through the
    /// internal setters. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log, the delays and the waits.</param>
    /// <param name="device">The kit's access to the device.</param>
    /// <param name="queue">The request queue.</param>
    /// <param name="block">The bounce block: header, status and data area.</param>
    /// <param name="blockCount">The disk's size in blocks.</param>
    /// <param name="blockSize">Bytes per block.</param>
    /// <param name="maxTransferBytes">The most bytes one request moves, a multiple of <paramref name="blockSize"/>.</param>
    /// <param name="readOnly">True when the device negotiated VIRTIO_BLK_F_RO.</param>
    /// <param name="flushNegotiated">True when the device negotiated VIRTIO_BLK_F_FLUSH.</param>
    /// <param name="index">The number of the disk's name.</param>
    internal VirtioBlkState(DeviceBinding binding, VirtioAccess device, Virtqueue queue, DmaBuffer block, ulong blockCount, uint blockSize, int maxTransferBytes, bool readOnly, bool flushNegotiated, uint index)
    {
        _binding = binding;
        _device = device;
        _queue = queue;
        _block = block;
        _blockCount = blockCount;
        _blockSize = blockSize;
        _maxTransferBytes = maxTransferBytes;
        _readOnly = readOnly;
        _flushNegotiated = flushNegotiated;
        _index = index;
        _name = "vblk" + index;
    }

    // --- IBlockDevice ---

    /// <summary><c>vblk&lt;n&gt;</c>, with the lowest index no live disk of the driver uses. Any context.</summary>
    public string Name => _name;

    /// <summary>The disk's size in blocks, from the configuration space's capacity. Any context.</summary>
    public ulong BlockCount => _blockCount;

    /// <summary>Bytes per block: <c>blk_size</c> when negotiated, else 512. Any context.</summary>
    public ulong BlockSize => _blockSize;

    // --- Properties the suite reads ---

    /// <summary>The number of the disk's name. Any context.</summary>
    public uint Index => _index;

    /// <summary>True when the device negotiated VIRTIO_BLK_F_RO; every write throws. Any context.</summary>
    public bool IsReadOnly => _readOnly;

    /// <summary>True when the device negotiated VIRTIO_BLK_F_FLUSH, so <see cref="Flush"/> sends a request. Any context.</summary>
    public bool FlushNegotiated => _flushNegotiated;

    /// <summary>The most bytes one request moves; a longer transfer is split. Any context.</summary>
    public int MaxTransferBytes => _maxTransferBytes;

    /// <summary>True when the device runs the virtio 1 interface, false on the legacy one. Any context.</summary>
    public bool Version1Negotiated => _device.Version1Negotiated;

    /// <summary>True when the queue interrupt is connected to <see cref="OnInterrupt"/>, so a request waits on the event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when no queue interrupt could be connected, so a request polls the used ring itself. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>How many requests had their chain submitted and notified. Any context.</summary>
    public int RequestsSubmitted => _requestsSubmitted;

    /// <summary>How many requests had their used element taken. Any context.</summary>
    public int RequestsCompleted => _requestsCompleted;

    /// <summary>How many requests went unanswered for <see cref="RequestTimeoutMilliseconds"/>. Any context.</summary>
    public int Timeouts => _timeouts;

    /// <summary>How many times the queue interrupt ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>True once a timeout or a protocol fault left a chain outstanding; every later request throws. Any context.</summary>
    public bool IsFaulted => _faulted;

    /// <summary>True from the first step of the binding's teardown on, forever. Any context.</summary>
    public bool IsDetached => _binding.IsDetaching;

    // --- Internal properties the probe sets ---

    /// <summary>The lock around the slot and the ring, created by the probe. Thread context.</summary>
    internal DeviceLock? Lock { get; set; }

    /// <summary>The event the queue interrupt signals, created by the probe. Thread context.</summary>
    internal DeviceEvent? Event { get; set; }

    // --- Interrupt ---

    /// <summary>The queue's handler: counts and signals the event; reaches nothing else. Interrupt context; allocation-free.</summary>
    /// <param name="context">What the handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        _interruptCount++;
        DeviceEvent? evt = Event;
        if (evt is not null)
        {
            context.Signal(evt);
        }
    }

    // --- IBlockDevice methods ---

    /// <summary>
    /// Reads whole blocks, in requests of at most
    /// <see cref="MaxTransferBytes"/> through the bounce block. Thread
    /// context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks asked for, or the range runs past the end of the disk.</exception>
    /// <exception cref="IOException">A request failed, timed out, or the device is faulted or being detached.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        CheckRange(blockNo, blockCount, data.Length);
        ulong blocksPerRequest = (ulong)_maxTransferBytes / _blockSize;
        ulong done = 0;
        while (done < blockCount)
        {
            ulong chunkBlocks = Math.Min(blockCount - done, blocksPerRequest);
            int chunkBytes = (int)(chunkBlocks * _blockSize);
            int offset = (int)(done * _blockSize);
            Request(VirtioBlkProtocol.RequestIn, SectorOf(blockNo + done), chunkBytes, data.Slice(offset, chunkBytes), default);
            done += chunkBlocks;
        }
    }

    /// <summary>
    /// Writes whole blocks, in requests of at most
    /// <see cref="MaxTransferBytes"/> through the bounce block. Thread
    /// context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Their bytes; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks given, or the range runs past the end of the disk.</exception>
    /// <exception cref="IOException">The device is read-only, a request failed, timed out, or the device is faulted or being detached.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        CheckRange(blockNo, blockCount, data.Length);
        if (_readOnly)
        {
            throw new IOException("the device is read-only");
        }

        ulong blocksPerRequest = (ulong)_maxTransferBytes / _blockSize;
        ulong done = 0;
        while (done < blockCount)
        {
            ulong chunkBlocks = Math.Min(blockCount - done, blocksPerRequest);
            int chunkBytes = (int)(chunkBlocks * _blockSize);
            int offset = (int)(done * _blockSize);
            Request(VirtioBlkProtocol.RequestOut, SectorOf(blockNo + done), chunkBytes, default, data.Slice(offset, chunkBytes));
            done += chunkBlocks;
        }
    }

    /// <summary>
    /// Sends the flush request when the device negotiated a volatile cache;
    /// a no-op otherwise, as the contract asks. A flush carries no data and
    /// names sector 0. Thread context; any thread.
    /// </summary>
    /// <exception cref="IOException">The request failed, timed out, or the device is faulted or being detached.</exception>
    public void Flush()
    {
        if (!_flushNegotiated)
        {
            return;
        }

        Request(VirtioBlkProtocol.RequestFlush, 0, 0, default, default);
    }

    // --- The request path ---

    /// <summary>
    /// The one request path. Claims the slot (under the lock, when no
    /// request is in flight; otherwise pauses and tries again until
    /// <see cref="BusyWaitMilliseconds"/>; a detach or a fault seen on any
    /// round ends the wait with its exception), writes the header, the pending
    /// status and the outgoing data into the bounce block, chains the
    /// header, the data and the status under the lock and submits and
    /// notifies. Then waits without the lock, up to
    /// <see cref="RequestTimeoutMilliseconds"/>, each round checking the
    /// detach first and taking the used ring under the lock: the chain's
    /// head ends the wait, a foreign id is a protocol fault, nothing used
    /// waits on the event with an interrupt or pauses without one. A
    /// detach, a timeout or a foreign id marks the state faulted and leaves
    /// the slot held for good, since the device may still write the block.
    /// Only after the used element is taken are the status and the data
    /// read; the slot is released after that copy. Thread context; any thread.
    /// </summary>
    /// <param name="type">The request type.</param>
    /// <param name="sector">The first sector.</param>
    /// <param name="dataBytes">Bytes of data the request moves; 0 for a flush.</param>
    /// <param name="destination">Where a read's data goes.</param>
    /// <param name="source">What a write carries.</param>
    /// <exception cref="IOException">The request failed, timed out, or the device is faulted or being detached.</exception>
    private void Request(uint type, ulong sector, int dataBytes, Span<byte> destination, ReadOnlySpan<byte> source)
    {
        // 1. The slot. Every round checks the detach and the fault under
        //    the lock before it claims, so a caller already waiting when the
        //    teardown begins or the holder faults leaves at once instead of
        //    claiming the slot over a released block or spinning to the
        //    deadline.
        DeviceLock deviceLock = Lock ?? throw new InvalidOperationException("The probe has not created the lock.");
        long slotDeadline = DeadlineAfter(BusyWaitMilliseconds);
        while (true)
        {
            bool claimed = false;
            bool detaching = false;
            bool faulted = false;
            using (deviceLock.Acquire())
            {
                if (_binding.IsDetaching)
                {
                    detaching = true;
                }
                else if (_faulted)
                {
                    faulted = true;
                }
                else if (!_busy)
                {
                    _busy = true;
                    claimed = true;
                }
            }

            if (detaching)
            {
                throw new IOException("virtio-blk device detached");
            }

            if (faulted)
            {
                throw new IOException("virtio-blk device faulted");
            }

            if (claimed)
            {
                break;
            }

            if (Stopwatch.GetTimestamp() >= slotDeadline)
            {
                throw new IOException("virtio-blk request slot busy");
            }

            _binding.Delay(PollMicroseconds);
        }

        // 2. The buffers, outside the lock: the block is the slot holder's.
        Span<byte> block = _block.Span;
        VirtioBlkRequestHeader header = new()
        {
            Type = type,
            Reserved = 0,
            Sector = sector,
        };
        MemoryMarshal.Write(block.Slice(VirtioBlkDriver.HeaderOffset), in header);
        block[VirtioBlkDriver.StatusOffset] = VirtioBlkProtocol.StatusPending;
        if (type == VirtioBlkProtocol.RequestOut)
        {
            source.CopyTo(block.Slice(VirtioBlkDriver.DataOffset, dataBytes));
        }

        // 3. The chain, under the lock. A teardown that began since the
        //    claim gets the slot back and nothing reaches the ring.
        ushort head = 0;
        long deadline;
        bool submitted = false;
        bool detachedBeforeSubmit = false;
        using (deviceLock.Acquire())
        {
            if (_binding.IsDetaching)
            {
                detachedBeforeSubmit = true;
            }
            else
            {
                submitted = TrySubmitLocked(type, dataBytes, out head);
            }

            if (!submitted)
            {
                _busy = false;
            }

            deadline = DeadlineAfter(RequestTimeoutMilliseconds);
        }

        if (detachedBeforeSubmit)
        {
            throw new IOException("virtio-blk device detached");
        }

        if (!submitted)
        {
            throw new IOException("no free descriptor on the request queue");
        }

        _requestsSubmitted++;

        // 4. The wait, without the lock. The detach check precedes every
        //    ring access: the teardown cancels the event, which wakes this
        //    waiter, before it frees the ring.
        bool taken = false;
        bool foreign = false;
        bool detached = false;
        while (true)
        {
            if (_binding.IsDetaching)
            {
                detached = true;
                break;
            }

            using (deviceLock.Acquire())
            {
                if (_queue.TryTakeUsed(out ushort id, out _))
                {
                    if (id == head)
                    {
                        FreeChainLocked(head);
                        taken = true;
                    }
                    else
                    {
                        _queue.FreeDescriptor(id);
                        foreign = true;
                    }
                }
            }

            if (taken || foreign)
            {
                break;
            }

            long now = Stopwatch.GetTimestamp();
            if (now >= deadline)
            {
                break;
            }

            if (HasInterrupt && Event is { } evt)
            {
                long remainingMilliseconds = (deadline - now) * MillisecondsPerSecond / Stopwatch.Frequency;
                _binding.Wait(evt, (uint)Math.Max(1, remainingMilliseconds));
            }
            else
            {
                _binding.Delay(PollMicroseconds);
            }
        }

        if (!taken)
        {
            // The slot stays held: the device may still write the block.
            using (deviceLock.Acquire())
            {
                _faulted = true;
            }

            if (detached)
            {
                throw new IOException("virtio-blk device detached");
            }

            if (foreign)
            {
                if (!_faultLogged)
                {
                    _faultLogged = true;
                    _binding.Log("unexpected used element, the device is faulted");
                }

                throw new IOException("virtio-blk protocol fault");
            }

            _timeouts++;
            throw new IOException("virtio-blk request timed out");
        }

        // 5. The status, after the used element, then the slot goes back.
        DmaBuffer.ReadBarrier();
        Span<byte> completed = _block.Span;
        byte status = completed[VirtioBlkDriver.StatusOffset];
        _requestsCompleted++;
        if (status == VirtioBlkProtocol.StatusOk && type == VirtioBlkProtocol.RequestIn)
        {
            completed.Slice(VirtioBlkDriver.DataOffset, dataBytes).CopyTo(destination);
        }

        using (deviceLock.Acquire())
        {
            _busy = false;
        }

        if (status == VirtioBlkProtocol.StatusOk)
        {
            return;
        }

        if (status == VirtioBlkProtocol.StatusIoError)
        {
            throw new IOException("virtio-blk I/O error");
        }

        if (status == VirtioBlkProtocol.StatusUnsupported)
        {
            throw new IOException("virtio-blk request unsupported");
        }

        throw new IOException($"virtio-blk status 0x{status:X2}");
    }

    /// <summary>
    /// Allocates the chain (the header, the data when there is any, the
    /// status), fills the descriptors over the bounce block, submits the
    /// head and notifies. Under the lock; allocation-free.
    /// </summary>
    /// <param name="type">The request type, which decides whether the device writes the data.</param>
    /// <param name="dataBytes">Bytes of data; 0 leaves the data descriptor out.</param>
    /// <param name="head">The chain's head, when submitted.</param>
    /// <returns>False when the queue has too few free descriptors; whatever was taken is freed and nothing was submitted.</returns>
    private bool TrySubmitLocked(uint type, int dataBytes, out ushort head)
    {
        bool hasData = dataBytes > 0;
        if (!_queue.TryAllocateDescriptor(out head))
        {
            return false;
        }

        ushort dataIndex = 0;
        if (hasData && !_queue.TryAllocateDescriptor(out dataIndex))
        {
            _queue.FreeDescriptor(head);
            return false;
        }

        if (!_queue.TryAllocateDescriptor(out ushort statusIndex))
        {
            _queue.FreeDescriptor(head);
            if (hasData)
            {
                _queue.FreeDescriptor(dataIndex);
            }

            return false;
        }

        ulong physical = _block.PhysicalAddress;
        _queue.SetDescriptor(head, physical + VirtioBlkDriver.HeaderOffset, VirtioBlkProtocol.HeaderBytes, VirtqueueDescriptorFlags.Next, hasData ? dataIndex : statusIndex);
        if (hasData)
        {
            VirtqueueDescriptorFlags dataFlags = type == VirtioBlkProtocol.RequestIn ? VirtqueueDescriptorFlags.Write : VirtqueueDescriptorFlags.None;
            _queue.SetDescriptor(dataIndex, physical + VirtioBlkDriver.DataOffset, (uint)dataBytes, dataFlags | VirtqueueDescriptorFlags.Next, statusIndex);
        }

        _queue.SetDescriptor(statusIndex, physical + VirtioBlkDriver.StatusOffset, VirtioBlkProtocol.StatusBytes, VirtqueueDescriptorFlags.Write);
        _inFlightHasData = hasData;
        _inFlightData = dataIndex;
        _inFlightStatus = statusIndex;
        _queue.Submit(head);
        _queue.Notify();
        return true;
    }

    /// <summary>Frees the in-flight chain's descriptors. Under the lock; allocation-free.</summary>
    /// <param name="head">The chain's head.</param>
    private void FreeChainLocked(ushort head)
    {
        _queue.FreeDescriptor(head);
        if (_inFlightHasData)
        {
            _queue.FreeDescriptor(_inFlightData);
        }

        _queue.FreeDescriptor(_inFlightStatus);
        _inFlightHasData = false;
    }

    /// <summary>
    /// Checks a block range against the caller's buffer and the disk. The
    /// first block is checked alone before the sum, so a block number near
    /// the top of the range cannot wrap past the end check. Any context.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="bufferLength">The caller's buffer length in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">The buffer is too short, or the range runs past the end of the disk.</exception>
    private void CheckRange(ulong blockNo, ulong blockCount, int bufferLength)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)bufferLength / _blockSize, nameof(blockCount));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockNo, _blockCount, nameof(blockNo));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockNo + blockCount, _blockCount, nameof(blockNo));
    }

    /// <summary>The sector a block starts at: the request's <c>sector</c> is in 512-byte units whatever the block size. Any context.</summary>
    /// <param name="block">The block number.</param>
    private ulong SectorOf(ulong block) => block * (_blockSize / VirtioBlkProtocol.SectorBytes);

    /// <summary>The timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks. Any context.</summary>
    /// <param name="milliseconds">How far ahead.</param>
    private static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;
}
