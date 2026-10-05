// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// The virtio block device over the driver kit: binds a virtio node of type
/// 2 under either transport, negotiates the block features, reads the
/// geometry, creates the request queue, and publishes the disk to the
/// storage manager as <c>vblk&lt;n&gt;</c>, which registers and scans it
/// inside the probe. One request is in flight at a time through one DMA
/// bounce block (<see cref="VirtioBlkState"/>). <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker; the
/// index of a disk's name is the lowest one no live disk of this driver
/// uses, so a disk plugged back in gets its name back.
/// </summary>
[Driver(Feature = DriverFeature.Storage)]
public sealed class VirtioBlkDriver : Driver
{
    // --- Constants ---

    /// <summary>The request queue size asked for: three descriptors per request; the device's maximum caps it, QEMU offers 256.</summary>
    private const ushort PreferredQueueSize = 16;

    /// <summary>Bytes of one page: the bounce block's alignment. The kit exposes no page size.</summary>
    internal const int PageBytes = 4096;

    /// <summary>Bytes of the bounce block's data area, 128 sectors: one request moves at most this much.</summary>
    internal const int TransferBytes = 65536;

    /// <summary>Offset of the request header in the bounce block.</summary>
    internal const int HeaderOffset = 0;

    /// <summary>Offset of the status byte in the bounce block, right after the header.</summary>
    internal const int StatusOffset = 16;

    /// <summary>Offset of the data area in the bounce block, page-aligned.</summary>
    internal const int DataOffset = 4096;

    /// <summary>Bytes of the bounce block: the header page and the data area, allocated page-aligned and physically contiguous.</summary>
    internal const int RequestBlockBytes = DataOffset + TransferBytes;

    /// <summary>How long the detach hook waits after the reset, so DMA in flight lands before the ring is freed.</summary>
    private const uint QuiesceMicroseconds = 100;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        VirtioMatch.DeviceType(VirtioDeviceType.Block),
    ];

    /// <summary>The live disks of this driver, for the name index; probes and detach hooks are serialized on the kit worker, so no lock.</summary>
    private readonly List<VirtioBlkState> _units = new();

    // --- Driver ---

    /// <inheritdoc/>
    public override string Name => nameof(VirtioBlkDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Brings the device up and publishes its disk. Thread context on the
    /// kit worker; a declined or failed result makes the kit release
    /// everything acquired here and reset the device through its hooks.
    /// </summary>
    /// <param name="binding">The virtio node and the kit facilities for it.</param>
    /// <returns>Bound with the disk published; declined when the device is healthy but not one this driver operates; failed when it did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The features. The three-descriptor chain is what the legacy
        //    interface requires and what a modern device accepts, so no
        //    framing decision follows.
        VirtioAccess dev = binding.Node.Access<VirtioAccess>();
        uint requested = VirtioBlkProtocol.FeatureSizeMax | VirtioBlkProtocol.FeatureSegMax | VirtioBlkProtocol.FeatureReadOnly
            | VirtioBlkProtocol.FeatureBlockSize | VirtioBlkProtocol.FeatureFlush | VirtioBlkProtocol.FeatureAnyLayout;
        if (!dev.NegotiateFeatures(requested, out uint features))
        {
            return ProbeResult.Failed("the device rejected the feature set");
        }

        // 2. The geometry. A field is read only when its feature was negotiated.
        ulong capacity = dev.ReadConfig32(VirtioBlkProtocol.CapacityLowOffset) | ((ulong)dev.ReadConfig32(VirtioBlkProtocol.CapacityHighOffset) << 32);
        if (capacity == 0)
        {
            return ProbeResult.Declined("zero capacity");
        }

        uint blockSize = (features & VirtioBlkProtocol.FeatureBlockSize) != 0 ? dev.ReadConfig32(VirtioBlkProtocol.BlockSizeOffset) : VirtioBlkProtocol.SectorBytes;
        if (blockSize < VirtioBlkProtocol.SectorBytes || blockSize > TransferBytes || (blockSize & (blockSize - 1)) != 0 || blockSize % VirtioBlkProtocol.SectorBytes != 0)
        {
            return ProbeResult.Declined($"unsupported block size {blockSize}");
        }

        // capacity is in 512-byte sectors and blockSize a power-of-two
        // multiple of 512: dividing first is exact and cannot overflow.
        ulong blockCount = capacity / (blockSize / VirtioBlkProtocol.SectorBytes);
        if (blockCount == 0)
        {
            return ProbeResult.Declined("capacity below one block");
        }

        bool readOnly = (features & VirtioBlkProtocol.FeatureReadOnly) != 0;
        bool flush = (features & VirtioBlkProtocol.FeatureFlush) != 0;
        uint sizeMax = (features & VirtioBlkProtocol.FeatureSizeMax) != 0 ? dev.ReadConfig32(VirtioBlkProtocol.SizeMaxOffset) : 0;
        int maxTransfer = TransferBytes;
        if (sizeMax != 0 && sizeMax < (uint)maxTransfer)
        {
            maxTransfer = (int)(sizeMax / blockSize * blockSize);
            if (maxTransfer < (int)blockSize)
            {
                return ProbeResult.Declined("size_max below one block");
            }
        }

        // One data segment per request is what the driver uses.
        if ((features & VirtioBlkProtocol.FeatureSegMax) != 0 && dev.ReadConfig32(VirtioBlkProtocol.SegMaxOffset) == 0)
        {
            return ProbeResult.Declined("seg_max is 0");
        }

        // 3. The request queue.
        if (!dev.TryCreateQueue(binding, VirtioBlkProtocol.RequestQueue, PreferredQueueSize, out Virtqueue? queue))
        {
            return ProbeResult.Failed("no request queue");
        }

        if (queue.Size < VirtioBlkState.DescriptorsPerRequest)
        {
            return ProbeResult.Failed("the request queue is too small");
        }

        // 4. The bounce block every request goes through.
        DmaBuffer block = binding.AllocateDma(RequestBlockBytes, PageBytes);

        // 5. The state, its lock and its event.
        VirtioBlkState state = new(binding, dev, queue, block, blockCount, blockSize, maxTransfer, readOnly, flush, FirstFreeIndex());
        binding.DriverState = state;
        state.Lock = binding.CreateLock();
        state.Event = binding.CreateEvent();

        // 6. The queue's source; without it the request path polls the
        //    used ring itself. No periodic item: the storage manager's
        //    partition scan reads through the disk inside the publish, on
        //    the kit worker, where a work item of that worker never runs.
        bool connected = binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioBlkProtocol.RequestQueue), state.OnInterrupt, out _);
        state.HasInterrupt = connected;
        state.IsPolling = !connected;

        // 7. DRIVER_OK before the first request.
        dev.SetDriverOk();

        // 8. The disk to the ring. A consumer that refuses throws out of the
        //    publish and the probe fails with the index still free.
        binding.PublishBlockDevice(state);
        _units.Add(state);

        // 9. The geometry and the completion mode in the log.
        string readOnlyText = readOnly ? ", read-only" : "";
        string flushText = flush ? "flush" : "no flush";
        string versionText = dev.Version1Negotiated ? "version 1" : "legacy";
        string wakeText = connected ? "interrupt" : "polling";
        binding.Log($"{blockCount} blocks of {blockSize} bytes{readOnlyText}, {flushText}, {versionText}, {wakeText}, {maxTransfer} bytes per request");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Frees the disk's name, then resets the device so it stops before the
    /// kit frees the ring and the bounce block, and waits a moment for DMA
    /// in flight. Thread context on the kit worker, after the kit withdrew
    /// the disk, disconnected the handle and cancelled the event; nothing
    /// is written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not VirtioBlkState state)
        {
            return;
        }

        _units.Remove(state);
        if (!reason.HardwarePresent)
        {
            return;
        }

        binding.Node.Access<VirtioAccess>().Reset();
        binding.Delay(QuiesceMicroseconds);
    }

    /// <summary>Lowest name number no live disk of this driver uses, so a disk plugged back in gets its name back, and two disks never share one. Thread context on the kit worker.</summary>
    private uint FirstFreeIndex()
    {
        for (uint index = 0; ; index++)
        {
            bool used = false;
            for (int i = 0; i < _units.Count && !used; i++)
            {
                used = _units[i].Index == index;
            }

            if (!used)
            {
                return index;
            }
        }
    }
}
