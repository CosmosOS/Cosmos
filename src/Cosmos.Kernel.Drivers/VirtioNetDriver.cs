// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio-net driver (virtio specification section 5.1) over the driver
/// kit: negotiates the MAC, status and any-layout features, creates the
/// receive and transmit queues on the kit's access, posts one buffer per
/// receive descriptor, reads the station address, connects the queue and
/// configuration sources when the transport delivers them and polls
/// otherwise, then publishes the interface to the ring. Everything it
/// holds for one device lives on a <see cref="VirtioNetState"/> in
/// <see cref="DeviceBinding.DriverState"/>. The transport underneath (PCI
/// or MMIO) is invisible here. Framing: the net header and the frame share
/// one descriptor, which is conformant with VIRTIO_F_VERSION_1 (arbitrary
/// framing, spec 2.6.4) or VIRTIO_F_ANY_LAYOUT (spec 5.1.6.6) and never
/// used without one; a legacy device without any-layout is declined, the
/// split-header framing is not implemented. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Network)]
public sealed class VirtioNetDriver : Driver
{
    /// <summary>VIRTIO_NET_F_MAC: the device has a station address in its configuration space.</summary>
    private const uint FeatureMac = 1u << 5;

    /// <summary>VIRTIO_NET_F_STATUS: the configuration space carries the link status.</summary>
    private const uint FeatureStatus = 1u << 16;

    /// <summary>VIRTIO_F_ANY_LAYOUT: the device accepts any descriptor layout; on the legacy interface the only way to keep the header and the frame in one descriptor.</summary>
    private const uint FeatureAnyLayout = 1u << 27;

    /// <summary>Bytes of the net header without VIRTIO_F_VERSION_1 (spec 5.1.6.1).</summary>
    private const int LegacyHeaderBytes = 10;

    /// <summary>Bytes of the net header with VIRTIO_F_VERSION_1: the num_buffers field is always present.</summary>
    private const int ModernHeaderBytes = 12;

    /// <summary>The queue size asked for; the device's maximum caps it.</summary>
    private const ushort QueueSize = 128;

    /// <summary>Offset of the station address in the configuration space.</summary>
    private const uint MacConfigOffset = 0;

    /// <summary>Bytes of a station address.</summary>
    private const int MacAddressBytes = 6;

    /// <summary>Period of the drain when the receive queue has no interrupt.</summary>
    private const uint DrainPeriodMilliseconds = 50;

    /// <summary>How long the detach hook waits after the reset, so DMA in flight lands before the rings are freed.</summary>
    private const uint QuiesceMicroseconds = 100;

    private readonly DeviceMatch[] _matches =
    [
        VirtioMatch.DeviceType(VirtioDeviceType.Network),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(VirtioNetDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Brings the device up and publishes it. Thread context on the kit
    /// worker; a declined or failed result makes the kit release everything
    /// acquired here and reset the device through its hooks.
    /// </summary>
    /// <param name="binding">The virtio node and the kit facilities for it.</param>
    /// <returns>Bound with the interface published; declined when the device is healthy but not one this driver operates; failed when it did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The features and the framing they allow.
        VirtioAccess dev = binding.Node.Access<VirtioAccess>();
        if (!dev.NegotiateFeatures(FeatureMac | FeatureStatus | FeatureAnyLayout, out uint features))
        {
            return ProbeResult.Failed("the device rejected the feature set");
        }

        int headerSize = dev.Version1Negotiated ? ModernHeaderBytes : LegacyHeaderBytes;
        bool anyLayout = (features & FeatureAnyLayout) != 0;
        if (!dev.Version1Negotiated && !anyLayout)
        {
            return ProbeResult.Declined("legacy device without VIRTIO_F_ANY_LAYOUT");
        }

        // 2. The two queues.
        if (!dev.TryCreateQueue(binding, VirtioNetState.ReceiveQueue, QueueSize, out Virtqueue? receiveQueue))
        {
            return ProbeResult.Failed("no receive queue");
        }

        if (!dev.TryCreateQueue(binding, VirtioNetState.TransmitQueue, QueueSize, out Virtqueue? transmitQueue))
        {
            return ProbeResult.Failed("no transmit queue");
        }

        // 3. One buffer per descriptor; every receive slot posted, not yet
        //    notified: the available ring may be filled before DRIVER_OK,
        //    the kick may not be sent until after it.
        DmaBuffer receiveBuffers = binding.AllocateDma(receiveQueue.Size * VirtioNetState.BufferBytes, VirtioNetState.BufferAlignment);
        DmaBuffer transmitBuffers = binding.AllocateDma(transmitQueue.Size * VirtioNetState.BufferBytes, VirtioNetState.BufferAlignment);
        for (int i = 0; i < receiveQueue.Size; i++)
        {
            if (!receiveQueue.TryAllocateDescriptor(out ushort slot))
            {
                return ProbeResult.Failed("the receive queue ran out of descriptors while posting");
            }

            ulong physical = receiveBuffers.PhysicalAddress + (ulong)(slot * VirtioNetState.BufferBytes);
            receiveQueue.SetDescriptor(slot, physical, VirtioNetState.BufferBytes, VirtqueueDescriptorFlags.Write);
            receiveQueue.Submit(slot);
        }

        // 4. The station address and the link.
        byte[] address = new byte[MacAddressBytes];
        bool hasMac = (features & FeatureMac) != 0;
        if (hasMac)
        {
            for (int i = 0; i < MacAddressBytes; i++)
            {
                address[i] = dev.ReadConfig8(MacConfigOffset + (uint)i);
            }
        }

        if (!hasMac || IsAllZero(address))
        {
            return ProbeResult.Declined("no MAC address");
        }

        MACAddress macAddress = new(address);
        bool statusNegotiated = (features & FeatureStatus) != 0;
        bool linkUp = !statusNegotiated || (dev.ReadConfig16(VirtioNetState.StatusConfigOffset) & VirtioNetState.LinkUpBit) != 0;

        // 5. The state, its lock and the drain. Scheduling the drain once
        //    tells whether a worker exists to run it: without one the
        //    device would never receive, so it is declined and the kit's
        //    hook resets it. With a worker the run lands after this probe.
        VirtioNetState state = new(binding, dev, receiveQueue, transmitQueue, receiveBuffers, transmitBuffers, headerSize, macAddress, anyLayout, statusNegotiated);
        state.LinkUp = linkUp;
        binding.DriverState = state;
        state.Lock = binding.CreateLock();
        WorkItem drain = binding.CreateWorkItem(state.Drain);
        state.DrainWork = drain;
        if (!drain.Schedule())
        {
            return ProbeResult.Declined("no kit worker to run the drain on");
        }

        // 6. The sources the transport delivers, and the periodic drain when
        //    the receive queue has none.
        bool receiveConnected = binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioNetState.ReceiveQueue), state.OnInterrupt, out _);
        binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioNetState.TransmitQueue), state.OnInterrupt, out _);
        binding.TryRequestInterrupt(dev.ConfigInterrupt, state.OnInterrupt, out _);
        state.HasInterrupt = receiveConnected;
        bool polling = false;
        if (!receiveConnected)
        {
            polling = binding.TrySchedulePeriodic(DrainPeriodMilliseconds, drain);
            if (!polling)
            {
                return ProbeResult.Declined("no interrupt and no timer to poll with");
            }
        }

        state.IsPolling = polling;

        // 7. DRIVER_OK, the one kick that starts the receive ring, then the ring.
        dev.SetDriverOk();
        receiveQueue.Notify();
        state.Sink = binding.PublishNetwork(state);

        // 8.
        string link = linkUp ? "up" : "down";
        string framing = dev.Version1Negotiated ? "version 1" : "legacy any-layout";
        string wake = receiveConnected ? $"interrupts: {dev.InterruptEntryCount} entries" : $"polling every {DrainPeriodMilliseconds} ms";
        binding.Log($"mac {macAddress}, link {link}, {framing}, {wake}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Resets the device so it stops before the kit frees the rings, then
    /// waits a moment for DMA in flight (the status read-back is inside the
    /// reset). Thread context on the kit worker; nothing is written when
    /// the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its handles are already disconnected.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not VirtioNetState || !reason.HardwarePresent)
        {
            return;
        }

        binding.Node.Access<VirtioAccess>().Reset();
        binding.Delay(QuiesceMicroseconds);
    }

    /// <summary>True when every byte is zero. Any context; allocation-free.</summary>
    private static bool IsAllZero(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != 0)
            {
                return false;
            }
        }

        return true;
    }
}
