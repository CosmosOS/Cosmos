// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Virtio MMIO transport (virtio spec section 4.2): a fixed per-slot register
/// window, supporting both legacy (version 1) and modern (version 2) register
/// flows. Slot discovery is platform-specific (device tree / QEMU virt
/// layout), so the platform initializer supplies the window address, the
/// interrupt line, and the line-interrupt wiring callback.
/// </summary>
internal sealed class VirtioMmioTransport : VirtioTransport
{
    /// <summary>Virtio MMIO magic value ("virt" in little endian).</summary>
    public const uint Magic = 0x74726976;

    // Register offsets (virtio spec section 4.2.2).
    private const uint RegMagic = 0x00;
    private const uint RegVersion = 0x04;
    private const uint RegDeviceId = 0x08;
    private const uint RegDeviceFeatures = 0x10;
    private const uint RegDeviceFeaturesSel = 0x14;
    private const uint RegDriverFeatures = 0x20;
    private const uint RegDriverFeaturesSel = 0x24;
    private const uint RegGuestPageSize = 0x28;   // Legacy: must set before using PFN
    private const uint RegQueueSel = 0x30;
    private const uint RegQueueNumMax = 0x34;
    private const uint RegQueueNum = 0x38;
    private const uint RegQueueAlign = 0x3c;      // Legacy: queue alignment (usually 4096)
    private const uint RegQueuePfn = 0x40;        // Legacy: queue page frame number
    private const uint RegQueueReady = 0x44;      // Modern only
    private const uint RegQueueNotify = 0x50;
    private const uint RegInterruptStatus = 0x60;
    private const uint RegInterruptAck = 0x64;
    private const uint RegStatus = 0x70;
    private const uint RegQueueDescLow = 0x80;
    private const uint RegQueueDescHigh = 0x84;
    private const uint RegQueueDriverLow = 0x90;
    private const uint RegQueueDriverHigh = 0x94;
    private const uint RegQueueDeviceLow = 0xa0;
    private const uint RegQueueDeviceHigh = 0xa4;
    private const uint RegConfig = 0x100;

    private readonly ulong _baseAddress;
    private readonly uint _intid;
    private readonly uint _version;
    private readonly uint _deviceType;
    private readonly VirtioIrqEnable _irqEnable;
    private VirtioInterruptHandler? _handler;
    private bool _irqWired;

    public override uint DeviceType => _deviceType;
    public override string TransportName => "MMIO";
    protected override bool SupportsFeaturesOk => _version >= 2;

    private VirtioMmioTransport(ulong baseAddress, uint intid, uint version, uint deviceType, VirtioIrqEnable irqEnable)
    {
        _baseAddress = baseAddress;
        _intid = intid;
        _version = version;
        _deviceType = deviceType;
        _irqEnable = irqEnable;
    }

    /// <summary>
    /// Probes one MMIO slot; returns a transport when a virtio device is present.
    /// </summary>
    public static VirtioMmioTransport? TryProbe(ulong slotBase, uint intid, VirtioIrqEnable irqEnable)
    {
        if (ReadReg(slotBase, RegMagic) != Magic)
        {
            return null;
        }

        uint deviceType = ReadReg(slotBase, RegDeviceId);
        if (deviceType == 0)
        {
            return null;
        }

        uint version = ReadReg(slotBase, RegVersion);
        return new VirtioMmioTransport(slotBase, intid, version, deviceType, irqEnable);
    }

    protected override void AfterReset()
    {
        // Legacy MMIO derives the used-ring offset from the guest page size;
        // it must be programmed before any queue PFN is written.
        if (_version == 1)
        {
            Write32(RegGuestPageSize, (uint)PageAllocator.PageSize);
        }
    }

    protected override byte GetStatus() => (byte)Read32(RegStatus);

    protected override void SetStatus(byte status) => Write32(RegStatus, status);

    protected override ulong ReadDeviceFeatures()
    {
        Write32(RegDeviceFeaturesSel, 0);
        ulong features = Read32(RegDeviceFeatures);
        if (_version >= 2)
        {
            Write32(RegDeviceFeaturesSel, 1);
            features |= (ulong)Read32(RegDeviceFeatures) << 32;
        }

        return features;
    }

    protected override void WriteDriverFeatures(ulong features)
    {
        Write32(RegDriverFeaturesSel, 0);
        Write32(RegDriverFeatures, (uint)features);
        if (_version >= 2)
        {
            Write32(RegDriverFeaturesSel, 1);
            Write32(RegDriverFeatures, (uint)(features >> 32));
        }
    }

    protected override bool IsQueueInUse(ushort index)
    {
        Write32(RegQueueSel, index);
        return _version == 1
            ? Read32(RegQueuePfn) != 0
            : Read32(RegQueueReady) != 0;
    }

    protected override uint GetQueueMaxSize(ushort index)
    {
        Write32(RegQueueSel, index);
        return Read32(RegQueueNumMax);
    }

    protected override bool ActivateQueue(ushort index, Virtqueue queue)
    {
        Write32(RegQueueSel, index);
        Write32(RegQueueNum, queue.QueueSize);

        if (_version == 1)
        {
            Write32(RegQueueAlign, (uint)PageAllocator.PageSize);
            Write32(RegQueuePfn, (uint)(queue.QueueBaseAddr / (uint)PageAllocator.PageSize));
        }
        else
        {
            Write32(RegQueueDescLow, (uint)queue.DescriptorTableAddr);
            Write32(RegQueueDescHigh, (uint)(queue.DescriptorTableAddr >> 32));
            Write32(RegQueueDriverLow, (uint)queue.AvailableRingAddr);
            Write32(RegQueueDriverHigh, (uint)(queue.AvailableRingAddr >> 32));
            Write32(RegQueueDeviceLow, (uint)queue.UsedRingAddr);
            Write32(RegQueueDeviceHigh, (uint)(queue.UsedRingAddr >> 32));
            Write32(RegQueueReady, 1);
        }

        return true;
    }

    public override void NotifyQueue(ushort index)
    {
        // Ring/descriptor writes are Normal memory, the doorbell is Device
        // memory — order them before the store the device acts on.
        PlatformHAL.Initializer?.DmaBarrier();
        Write32(RegQueueNotify, index);
    }

    public override uint ReadAndAckIsr()
    {
        uint status = Read32(RegInterruptStatus);
        if (status != 0)
        {
            Write32(RegInterruptAck, status);
        }

        return status;
    }

    public override bool EnableInterrupt(VirtioInterruptHandler handler)
    {
        _handler = handler;
        if (!_irqWired)
        {
            Serial.Write("[VirtioMmio] Wiring INTID ");
            Serial.WriteNumber(_intid);
            Serial.Write("\n");
            _irqEnable(_intid, HandleIrq);
            _irqWired = true;
        }

        return true;
    }

    private void HandleIrq(ref IRQContext context)
    {
        // ALWAYS acknowledge: virtio MMIO interrupts are level-triggered, and
        // an unacked line re-fires immediately (IRQ storm).
        uint status = ReadAndAckIsr();
        _handler?.Invoke(status);
    }

    public override byte ReadDeviceConfig8(uint offset) =>
        Native.MMIO.Read8(VirtioDma.PhysToVirt(_baseAddress + RegConfig + offset));

    public override ushort ReadDeviceConfig16(uint offset) =>
        Native.MMIO.Read16(VirtioDma.PhysToVirt(_baseAddress + RegConfig + offset));

    public override void WriteDeviceConfig8(uint offset, byte value) =>
        Native.MMIO.Write8(VirtioDma.PhysToVirt(_baseAddress + RegConfig + offset), value);

    // Register access goes through the HHDM alias so the ARM64 Device-memory
    // mapping installed by DeviceMapper applies (see VirtioDma.PhysToVirt).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadReg(ulong baseAddr, uint offset) =>
        Native.MMIO.Read32(VirtioDma.PhysToVirt(baseAddr + offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint Read32(uint offset) => ReadReg(_baseAddress, offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write32(uint offset, uint value) =>
        Native.MMIO.Write32(VirtioDma.PhysToVirt(_baseAddress + offset), value);
}
