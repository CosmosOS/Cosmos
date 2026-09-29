// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="VirtioNetDriver"/> holds for one bound device,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the network
/// interface it publishes: the access, the two queues and their buffers,
/// the kit lock, the sink, the drain work item and the counters the Virtio
/// suite reads. <see cref="Transmit"/> is entered by the ring from its own
/// thread and again from the drain's synchronous replies, so it runs under
/// the <see cref="DeviceLock"/>; <see cref="Drain"/> runs on the kit worker
/// and takes the lock only around its own ring operations, never across a
/// sink call; <see cref="OnInterrupt"/> runs in interrupt context and only
/// counts and schedules the drain.
/// </summary>
public sealed class VirtioNetState : INetworkInterface
{
    /// <summary>The receive queue's index (receiveq1).</summary>
    internal const ushort ReceiveQueue = 0;

    /// <summary>The transmit queue's index (transmitq1).</summary>
    internal const ushort TransmitQueue = 1;

    /// <summary>Bytes of one buffer: the net header and a frame; also the transmit length cap.</summary>
    internal const int BufferBytes = 2048;

    /// <summary>Alignment of a buffer area: one page.</summary>
    internal const int BufferAlignment = 4096;

    /// <summary>Offset of the link status word in the configuration space.</summary>
    internal const uint StatusConfigOffset = 6;

    /// <summary>VIRTIO_NET_S_LINK_UP within the status word.</summary>
    internal const ushort LinkUpBit = 1;

    private readonly DeviceBinding _binding;
    private readonly VirtioAccess _access;
    private readonly Virtqueue _receiveQueue;
    private readonly Virtqueue _transmitQueue;
    private readonly DmaBuffer _receiveBuffers;
    private readonly DmaBuffer _transmitBuffers;
    private readonly int _headerSize;
    private readonly MACAddress _macAddress;
    private readonly bool _anyLayoutNegotiated;
    private readonly bool _statusNegotiated;
    private DeviceLock? _lock;
    private NetworkSink? _sink;
    private WorkItem? _drainWork;
    private volatile bool _linkUp;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile int _framesReceived;
    private volatile int _framesTransmitted;
    private volatile int _interruptCount;
    private volatile int _linkChanges;

    /// <summary>
    /// Takes the access, the queues and the buffers the probe acquired; the
    /// receive slots are already posted. Thread context, from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="access">The kit's access to the device.</param>
    /// <param name="receiveQueue">Queue 0, every descriptor posted with its slot.</param>
    /// <param name="transmitQueue">Queue 1, every descriptor free.</param>
    /// <param name="receiveBuffers">One slot of <see cref="BufferBytes"/> per receive descriptor.</param>
    /// <param name="transmitBuffers">One slot of <see cref="BufferBytes"/> per transmit descriptor.</param>
    /// <param name="headerSize">Bytes of the net header in front of every frame.</param>
    /// <param name="macAddress">The station address.</param>
    /// <param name="anyLayoutNegotiated">Whether VIRTIO_F_ANY_LAYOUT was negotiated.</param>
    /// <param name="statusNegotiated">Whether VIRTIO_NET_F_STATUS was negotiated, so the drain reads the link.</param>
    internal VirtioNetState(DeviceBinding binding, VirtioAccess access, Virtqueue receiveQueue, Virtqueue transmitQueue, DmaBuffer receiveBuffers, DmaBuffer transmitBuffers, int headerSize, MACAddress macAddress, bool anyLayoutNegotiated, bool statusNegotiated)
    {
        _binding = binding;
        _access = access;
        _receiveQueue = receiveQueue;
        _transmitQueue = transmitQueue;
        _receiveBuffers = receiveBuffers;
        _transmitBuffers = transmitBuffers;
        _headerSize = headerSize;
        _macAddress = macAddress;
        _anyLayoutNegotiated = anyLayoutNegotiated;
        _statusNegotiated = statusNegotiated;
    }

    /// <inheritdoc/>
    public string Name => "virtio-net";

    /// <inheritdoc/>
    public MACAddress MacAddress => _macAddress;

    /// <summary>True while the link is up, as the probe read it and the drain keeps it. Any context.</summary>
    public bool LinkUp
    {
        get => _linkUp;
        internal set => _linkUp = value;
    }

    /// <summary>How many frames the drain handed to the ring. Any context.</summary>
    public int FramesReceived => _framesReceived;

    /// <summary>How many frames <see cref="Transmit"/> queued. Any context.</summary>
    public int FramesTransmitted => _framesTransmitted;

    /// <summary>How many times a connected source ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many times the drain saw the link change state. Any context.</summary>
    public int LinkChanges => _linkChanges;

    /// <summary>True when the receive queue's source is connected to <see cref="OnInterrupt"/>. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when the kit runs the drain periodically because the receive queue has no interrupt. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True when the kit negotiated VIRTIO_F_VERSION_1 with the device. Any context.</summary>
    public bool Version1Negotiated => _access.Version1Negotiated;

    /// <summary>True when VIRTIO_F_ANY_LAYOUT was negotiated. Any context.</summary>
    public bool AnyLayoutNegotiated => _anyLayoutNegotiated;

    /// <summary>How many interrupt entries the transport delivers to the device; 0 when it polls. Any context.</summary>
    public int InterruptEntryCount => _access.InterruptEntryCount;

    /// <summary>The lock <see cref="Transmit"/> and the drain's ring operations run under; set by the probe before the drain is scheduled.</summary>
    internal DeviceLock? Lock
    {
        get => _lock;
        set => _lock = value;
    }

    /// <summary>The sink frames and link changes go to; set by the probe when the interface is published.</summary>
    internal NetworkSink? Sink
    {
        get => _sink;
        set => _sink = value;
    }

    /// <summary>The work item running <see cref="Drain"/>, which the handler schedules; set by the probe before a source is connected.</summary>
    internal WorkItem? DrainWork
    {
        get => _drainWork;
        set => _drainWork = value;
    }

    /// <summary>
    /// Queues one frame on the transmit queue. False before the probe
    /// created the lock; otherwise, under the lock: the used transmit
    /// descriptors are reclaimed first; false when the frame is empty,
    /// longer than a slot minus the header, or no descriptor is free; else
    /// a zeroed header and the frame are copied into the descriptor's slot,
    /// the descriptor written, submitted and the device notified. Any
    /// context; called by the ring from its own thread and from the drain's
    /// synchronous replies.
    /// </summary>
    /// <param name="frame">The frame, without checksum.</param>
    /// <returns>True when the frame was queued.</returns>
    public bool Transmit(ReadOnlySpan<byte> frame)
    {
        DeviceLock? deviceLock = _lock;
        if (deviceLock is null || frame.Length == 0 || frame.Length > BufferBytes - _headerSize)
        {
            return false;
        }

        using (deviceLock.Acquire())
        {
            ReclaimTransmitLocked();
            if (!_transmitQueue.TryAllocateDescriptor(out ushort index))
            {
                return false;
            }

            Span<byte> slot = _transmitBuffers.Span.Slice(index * BufferBytes, BufferBytes);
            slot.Slice(0, _headerSize).Clear();
            frame.CopyTo(slot.Slice(_headerSize, frame.Length));
            ulong physical = _transmitBuffers.PhysicalAddress + (ulong)(index * BufferBytes);
            _transmitQueue.SetDescriptor(index, physical, (uint)(_headerSize + frame.Length), VirtqueueDescriptorFlags.None);
            _transmitQueue.Submit(index);
            _transmitQueue.Notify();
            _framesTransmitted++;
            return true;
        }
    }

    /// <summary>
    /// A source's handler: counts the interrupt and schedules the drain.
    /// Interrupt context; allocation-free.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        _interruptCount++;
        WorkItem? drain = _drainWork;
        if (drain is not null)
        {
            context.Schedule(drain);
        }
    }

    /// <summary>
    /// The drain: reports a link change when the status feature was
    /// negotiated, then takes every used receive element, hands the frame
    /// behind the header to the sink outside the lock (the consumer copies
    /// it before returning, and the slot is re-posted only afterwards, so
    /// the device cannot write it meanwhile), re-posts the slot, kicks the
    /// receive queue once when anything was taken, and reclaims the used
    /// transmit descriptors. Idempotent, so a source and the periodic
    /// schedule can both run it. Thread context on the kit worker.
    /// </summary>
    internal void Drain()
    {
        DeviceLock? deviceLock = _lock;
        if (deviceLock is null)
        {
            return;
        }

        NetworkSink? sink = _sink;
        if (_statusNegotiated)
        {
            bool up = (_access.ReadConfig16(StatusConfigOffset) & LinkUpBit) != 0;
            if (up != _linkUp)
            {
                _linkUp = up;
                _linkChanges++;
                sink?.LinkChanged(up);
            }
        }

        bool taken = false;
        while (true)
        {
            ushort id;
            uint length;
            bool used;
            using (deviceLock.Acquire())
            {
                used = _receiveQueue.TryTakeUsed(out id, out length);
            }

            if (!used)
            {
                break;
            }

            taken = true;
            int bytes = Math.Min((int)length, BufferBytes);
            if (bytes > _headerSize)
            {
                ReadOnlySpan<byte> payload = _receiveBuffers.Span.Slice(id * BufferBytes + _headerSize, bytes - _headerSize);
                Deliver(sink, payload);
                _framesReceived++;
            }

            using (deviceLock.Acquire())
            {
                ulong physical = _receiveBuffers.PhysicalAddress + (ulong)(id * BufferBytes);
                _receiveQueue.SetDescriptor(id, physical, BufferBytes, VirtqueueDescriptorFlags.Write);
                _receiveQueue.Submit(id);
            }
        }

        if (taken)
        {
            _receiveQueue.Notify();
        }

        using (deviceLock.Acquire())
        {
            ReclaimTransmitLocked();
        }
    }

    /// <summary>
    /// Hands one frame to the sink, fencing whatever the ring does with it:
    /// the kit cancels a work item that throws, which would stop every later
    /// drain and leave the slot with the driver for good, so a delivery that
    /// throws is logged and the walk goes on. Thread context on the kit
    /// worker, outside the lock.
    /// </summary>
    private void Deliver(NetworkSink? sink, ReadOnlySpan<byte> payload)
    {
        if (sink is null)
        {
            return;
        }

        try
        {
            sink.Receive(payload);
        }
        catch (Exception exception)
        {
            _binding.Log($"a frame's delivery threw: {exception.Message}");
        }
    }

    /// <summary>Frees every transmit descriptor the device returned. Under the lock; any context; allocation-free.</summary>
    private void ReclaimTransmitLocked()
    {
        while (_transmitQueue.TryTakeUsed(out ushort id, out _))
        {
            _transmitQueue.FreeDescriptor(id);
        }
    }
}
