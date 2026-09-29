// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// The access object of a virtio node, the one thing a leaf driver sees:
/// the status handshake, the feature negotiation, the virtqueues and the
/// device configuration space, run once here over whatever
/// <see cref="VirtioTransport"/> the transport driver built. Behind it,
/// for the kit only, the bus hooks: the device is reset and brought to
/// DRIVER state before the first probe and again after a probe that did
/// not bind, left reset when nobody binds, and reset after a binding is
/// torn down. The node's interrupt sources are the access's own nine
/// objects (the configuration change, then one per queue), multiplexed
/// over the transport's entries by <see cref="Dispatch"/>. Constructed by
/// a transport driver in Cosmos.Kernel.Drivers, so the constructor is
/// public. The execution context of each member is in its summary.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class VirtioAccess : INodeHooks
{
    /// <summary>How many queues the kit describes per device; a queue index at or above it is refused.</summary>
    public const int MaxQueues = 8;

    /// <summary>
    /// The longest the kit waits for a device to acknowledge a reset before
    /// continuing: QEMU resets synchronously, a vhost or hardware backend
    /// may not, and virtio 1.1 sections 4.1.4.3.2 and 4.2.2.2 require the
    /// status read-back.
    /// </summary>
    public const uint ResetTimeoutMilliseconds = 100;

    /// <summary>How long the reset read-back loop waits between two status reads.</summary>
    public const uint ResetPollMicroseconds = 10;

    /// <summary>The entry the configuration change source is assigned at the handshake start.</summary>
    private const int ConfigEntry = 0;

    private readonly VirtioTransport _transport;
    private readonly VirtioInterruptSource _configSource;
    private readonly VirtioInterruptSource[] _queueSources;

    /// <summary>Wraps a transport.</summary>
    /// <param name="transport">The transport driver's registers.</param>
    public VirtioAccess(VirtioTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        _transport = transport;
        _configSource = VirtioInterruptSource.ForConfig();
        _queueSources = new VirtioInterruptSource[MaxQueues];
        for (int i = 0; i < MaxQueues; i++)
        {
            _queueSources[i] = VirtioInterruptSource.ForQueue(i);
        }
    }

    /// <summary>The device type the transport read. Any context.</summary>
    public VirtioDeviceType DeviceType => _transport.DeviceType;

    /// <summary>True once <see cref="NegotiateFeatures"/> took VIRTIO_F_VERSION_1 (bit 32), which the kit takes whenever the device offers it; false after a reset. Any context.</summary>
    public bool Version1Negotiated { get; private set; }

    /// <summary>How many interrupt entries the transport delivers, for the log: 0 when the leaf has to poll. Any context.</summary>
    public int InterruptEntryCount => _transport.InterruptEntryCount;

    /// <summary>
    /// The configuration change source, for a leaf to pass to
    /// <see cref="DeviceBinding.TryRequestInterrupt"/>. Connected only
    /// once its entry is assigned, at the handshake start; otherwise the
    /// request returns false and the leaf polls. Any context.
    /// </summary>
    public InterruptSource ConfigInterrupt => _configSource;

    /// <summary>
    /// The source of queue <paramref name="index"/>, for a leaf to pass to
    /// <see cref="DeviceBinding.TryRequestInterrupt"/>. Connected only once
    /// a successful <see cref="TryCreateQueue"/> assigned its entry;
    /// otherwise the request returns false and the leaf polls. Any context.
    /// </summary>
    /// <param name="index">The queue index.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is at or above <see cref="MaxQueues"/>.</exception>
    public InterruptSource QueueInterrupt(ushort index)
    {
        ThrowIfNotAQueue(index);
        return _queueSources[index];
    }

    /// <summary>
    /// The node's interrupt sources in the fixed order: index 0 the
    /// configuration change, index 1 + n queue n. A fresh array each call,
    /// for the transport driver to hand to
    /// <see cref="DeviceBinding.PublishChild"/> exactly once: the elements
    /// are the access's own source objects, so the node owns exactly these,
    /// and the access never reads the array itself, so a leaf cannot
    /// corrupt what the kit's ownership check sees. Thread context.
    /// </summary>
    public InterruptSource[] InterruptsForPublish()
    {
        InterruptSource[] sources = new InterruptSource[1 + MaxQueues];
        sources[0] = _configSource;
        for (int i = 0; i < MaxQueues; i++)
        {
            sources[1 + i] = _queueSources[i];
        }

        return sources;
    }

    /// <summary>
    /// Negotiates the features: the device's offer is masked with
    /// <paramref name="requestedLow"/>, the whole low word, reserved bits
    /// included (VIRTIO_F_ANY_LAYOUT, bit 27, is a leaf's request); the kit
    /// adds VIRTIO_F_VERSION_1 when offered and nothing else. On a
    /// transport with the FEATURES_OK step, sets it and reads it back.
    /// Thread context, after the hooks began the handshake.
    /// </summary>
    /// <param name="requestedLow">The low 32 feature bits the leaf understands.</param>
    /// <param name="negotiatedLow">The low 32 bits both sides accepted.</param>
    /// <returns>False when the device did not accept the feature set (FEATURES_OK did not stick).</returns>
    public bool NegotiateFeatures(uint requestedLow, out uint negotiatedLow)
    {
        ulong offered = _transport.ReadDeviceFeatures();
        ulong accepted = offered & requestedLow;
        if ((offered & VirtioStatus.Version1) != 0)
        {
            accepted |= VirtioStatus.Version1;
        }

        _transport.WriteDriverFeatures(accepted);
        Version1Negotiated = (accepted & VirtioStatus.Version1) != 0;
        negotiatedLow = (uint)accepted;

        if (_transport.SupportsFeaturesOk)
        {
            SetStatusBit(VirtioStatus.FeaturesOk);
            if ((_transport.ReadStatus() & VirtioStatus.FeaturesOk) == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Creates queue <paramref name="index"/> in DMA memory on
    /// <paramref name="binding"/>'s ledger and activates it on the device.
    /// The size is the smaller of the device's maximum and
    /// <paramref name="preferredSize"/>; both are powers of two by the
    /// specification (QEMU offers 256 or 1024) and the kit does not round.
    /// The descriptor table is at the start of a page-aligned block, the
    /// available ring right after it, the used ring on the next page
    /// boundary. The queue gets an interrupt entry when the transport has
    /// one for it: entry index + 1 when the transport has that many,
    /// otherwise entry 0, shared. Thread context.
    /// </summary>
    /// <param name="binding">The leaf's binding: the queue memory goes on its ledger and is freed with it.</param>
    /// <param name="index">The queue index.</param>
    /// <param name="preferredSize">The largest size the leaf wants.</param>
    /// <param name="queue">The queue, when created.</param>
    /// <returns>False when the index is at or above <see cref="MaxQueues"/>, the queue does not exist, is already ready, or the transport refused the layout; a refused layout leaves the memory on the ledger for the binding's unwind.</returns>
    /// <exception cref="ArgumentException"><paramref name="binding"/> is bound to a node whose access is not this one.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="preferredSize"/> is 0.</exception>
    /// <exception cref="InvalidOperationException">No pages left, the binding is being torn down, or the caller is an interrupt handler.</exception>
    public bool TryCreateQueue(DeviceBinding binding, ushort index, ushort preferredSize, [NotNullWhen(true)] out Virtqueue? queue)
    {
        ArgumentNullException.ThrowIfNull(binding);
        if (!ReferenceEquals(binding.Node.AccessObject, this))
        {
            throw new ArgumentException("The binding is bound to another node.", nameof(binding));
        }

        ArgumentOutOfRangeException.ThrowIfZero(preferredSize);
        queue = null;
        if (index >= MaxQueues)
        {
            return false;
        }

        ushort maxSize = _transport.ReadQueueMaxSize(index);
        if (maxSize == 0)
        {
            return false;
        }

        if (_transport.IsQueueReady(index))
        {
            return false;
        }

        ushort size = maxSize < preferredSize ? maxSize : preferredSize;
        ulong pageSize = PageAllocator.PageSize;
        ulong descriptorBytes = (ulong)Virtqueue.DescriptorBytes * size;
        ulong availableBytes = Virtqueue.AvailableRingHeaderBytes + (ulong)Virtqueue.AvailableRingEntryBytes * size + Virtqueue.AvailableRingTrailerBytes;
        ulong usedOffset = RoundUp(descriptorBytes + availableBytes, pageSize);
        ulong usedBytes = Virtqueue.UsedRingHeaderBytes + (ulong)Virtqueue.UsedRingElementBytes * size + Virtqueue.UsedRingTrailerBytes;
        ulong totalBytes = RoundUp(usedOffset + usedBytes, pageSize);
        DmaBuffer memory = binding.AllocateDma((int)totalBytes, (int)pageSize);

        int entry = ChooseQueueEntry(index);
        if (entry != VirtioInterruptSource.NoEntry && !_transport.AssignQueueInterrupt(index, entry))
        {
            entry = VirtioInterruptSource.NoEntry;
        }

        ulong physicalBase = memory.PhysicalAddress;
        VirtqueueLayout layout = new(size, physicalBase, physicalBase + descriptorBytes, physicalBase + usedOffset, physicalBase, (uint)pageSize);
        if (!_transport.ActivateQueue(index, in layout))
        {
            return false;
        }

        _queueSources[index].AssignEntry(entry);
        queue = new Virtqueue(_transport, index, size, memory, descriptorBytes, usedOffset);
        return true;
    }

    /// <summary>
    /// Sets DRIVER_OK: the driver is ready. A leaf calls
    /// <see cref="Virtqueue.Notify"/> on its queues only after this (virtio
    /// 3.1.1: no available buffer notification before DRIVER_OK);
    /// submitting descriptors before it is fine. Thread context.
    /// </summary>
    public void SetDriverOk() => SetStatusBit(VirtioStatus.DriverOk);

    /// <summary>Sets FAILED: the driver gave up on the device. Thread context.</summary>
    public void SetFailed() => SetStatusBit(VirtioStatus.Failed);

    /// <summary>
    /// Resets the device: writes status 0, waits up to
    /// <see cref="ResetTimeoutMilliseconds"/> for it to read back as 0 (a
    /// device that never answers is logged and treated as reset; the
    /// handshake that follows fails visibly), then lets the transport
    /// program what follows a reset, forgets the negotiated version and
    /// takes the entry away from every source (the reset put every vector
    /// register back to NO_VECTOR, so the kit's entries follow). Public so
    /// a leaf's OnDetach stops the device before the kit frees its rings:
    /// safe there because teardown disconnects the binding's handles
    /// before OnDetach, and safe before a decline because the bus hook and
    /// the unwind follow; a source the unwind disconnects afterwards stays
    /// disconnected. The kit's after-teardown hook resets again as a
    /// backstop. Thread context.
    /// </summary>
    public void Reset()
    {
        _transport.WriteStatus(0);
        long deadline = KitTime.DeadlineAfter(ResetTimeoutMilliseconds);
        while (_transport.ReadStatus() != 0)
        {
            if (KitTime.HasPassed(deadline))
            {
                DriverLog.VirtioResetTimedOut(DeviceType, ResetTimeoutMilliseconds);
                break;
            }

            KitTime.Delay(ResetPollMicroseconds);
        }

        _transport.AfterReset();
        Version1Negotiated = false;
        _configSource.AssignEntry(VirtioInterruptSource.NoEntry);
        for (int i = 0; i < MaxQueues; i++)
        {
            _queueSources[i].AssignEntry(VirtioInterruptSource.NoEntry);
        }
    }

    /// <summary>Reads one byte of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public byte ReadConfig8(uint offset) => _transport.ReadConfig8(offset);

    /// <summary>Reads one word of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public ushort ReadConfig16(uint offset) => _transport.ReadConfig16(offset);

    /// <summary>Reads one dword of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    public uint ReadConfig32(uint offset) => _transport.ReadConfig32(offset);

    /// <summary>Writes one byte of the device-specific configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the configuration space.</param>
    /// <param name="value">The value.</param>
    public void WriteConfig8(uint offset, byte value) => _transport.WriteConfig8(offset, value);

    /// <summary>
    /// Delivers one interrupt of the transport's entry
    /// <paramref name="entry"/>: reads and acknowledges the status, raises
    /// the configuration source when the status has a configuration change
    /// and that source's entry is <paramref name="entry"/>, raises every
    /// queue source whose entry is <paramref name="entry"/> when the status
    /// has a queue update. A source with no entry is never raised. A raise
    /// runs the leaf's trampoline, nested inside the transport's own
    /// dispatch. Called by the transport driver's handler; interrupt
    /// context; allocation-free.
    /// </summary>
    /// <param name="entry">The transport entry that fired, from 0.</param>
    public void Dispatch(int entry)
    {
        if (entry < 0)
        {
            return;
        }

        VirtioInterruptStatus status = _transport.ReadAndAcknowledgeInterrupt();
        if ((status & VirtioInterruptStatus.Config) != 0 && _configSource.Entry == entry)
        {
            _configSource.Raise();
        }

        if ((status & VirtioInterruptStatus.Queue) == 0)
        {
            return;
        }

        for (int i = 0; i < MaxQueues; i++)
        {
            VirtioInterruptSource source = _queueSources[i];
            if (source.Entry == entry)
            {
                source.Raise();
            }
        }
    }

    /// <inheritdoc/>
    void INodeHooks.BeforeFirstOffer() => Begin();

    /// <inheritdoc/>
    void INodeHooks.AfterOfferDeclined()
    {
        // The declined probe's trampoline is still connected to the config
        // source until the unwind, and Begin gives that source entry 0
        // again: mask it first so a configuration change landing in that
        // window is dropped at the kit. The unwind then drops the
        // trampoline, and the next candidate's connect clears the mask.
        _configSource.Quiet();
        Begin();
    }

    /// <inheritdoc/>
    void INodeHooks.AfterUnbound() => Reset();

    /// <inheritdoc/>
    void INodeHooks.AfterTeardown(bool hardwarePresent)
    {
        if (hardwarePresent)
        {
            Reset();
        }
    }

    /// <summary>
    /// Starts the handshake for the next candidate: a reset, the
    /// configuration source's entry when the transport has one and the
    /// device takes it, then ACKNOWLEDGE and DRIVER. Every candidate sees a
    /// freshly reset device in DRIVER state. Runs before a declined probe's
    /// handles are disconnected: the reset clears every entry first, and
    /// the queue sources stay at -1 until the unwind dropped the declined
    /// probe's trampolines, so none of them is raised; the configuration
    /// source gets entry 0 back here, so the declined hook masks it before
    /// calling this, and the unwind then drops its trampoline. Worker only.
    /// </summary>
    private void Begin()
    {
        Reset();
        if (_transport.InterruptEntryCount > 0 && _transport.AssignConfigInterrupt(ConfigEntry))
        {
            _configSource.AssignEntry(ConfigEntry);
        }

        _transport.WriteStatus(VirtioStatus.Acknowledge);
        SetStatusBit(VirtioStatus.Driver);
    }

    /// <summary>Ors <paramref name="bit"/> into the status register.</summary>
    private void SetStatusBit(byte bit) => _transport.WriteStatus((byte)(_transport.ReadStatus() | bit));

    /// <summary>The entry queue <paramref name="index"/> gets: index + 1 when the transport has that many, else 0 when it has any, else none.</summary>
    private int ChooseQueueEntry(ushort index)
    {
        int entryCount = _transport.InterruptEntryCount;
        if (index + 1 < entryCount)
        {
            return index + 1;
        }

        return entryCount > 0 ? 0 : VirtioInterruptSource.NoEntry;
    }

    /// <summary>Rounds <paramref name="value"/> up to the next multiple of <paramref name="boundary"/>. Any context.</summary>
    private static ulong RoundUp(ulong value, ulong boundary) => (value + boundary - 1) / boundary * boundary;

    /// <summary>Refuses a queue index at or above <see cref="MaxQueues"/>. Any context.</summary>
    private static void ThrowIfNotAQueue(ushort index)
    {
        if (index >= MaxQueues)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "The queue index is below MaxQueues.");
        }
    }
}
