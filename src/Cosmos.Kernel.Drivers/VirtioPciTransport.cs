// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio PCI transport (virtio specification section 4.1, the modern
/// interface): the common, notify, ISR and device configuration structures
/// the function's vendor capabilities point into, reached through the
/// register windows <see cref="VirtioPciTransportDriver"/> mapped over the
/// BARs. The kit's <see cref="VirtioAccess"/> calls the members; the driver
/// only sets <see cref="InterruptEntryCount"/> before it publishes the node.
/// Every register access goes through a <see cref="RegisterWindow"/>, whose
/// bounds are checked in every build, so no write can leave a BAR. The
/// execution context of each member is in its summary.
/// </summary>
public sealed class VirtioPciTransport : VirtioTransport
{
    /// <summary>device_feature_select: which 32-bit half <see cref="CommonDeviceFeature"/> shows.</summary>
    private const ulong CommonDeviceFeatureSelect = 0x00;

    /// <summary>device_feature: the selected half of the device's feature bits.</summary>
    private const ulong CommonDeviceFeature = 0x04;

    /// <summary>driver_feature_select: which 32-bit half <see cref="CommonDriverFeature"/> writes.</summary>
    private const ulong CommonDriverFeatureSelect = 0x08;

    /// <summary>driver_feature: the selected half of the driver's accepted feature bits.</summary>
    private const ulong CommonDriverFeature = 0x0C;

    /// <summary>msix_config: the MSI-X entry that signals a configuration change.</summary>
    private const ulong CommonMsixConfig = 0x10;

    /// <summary>device_status: the status register.</summary>
    private const ulong CommonDeviceStatus = 0x14;

    /// <summary>queue_select: which queue the registers from <see cref="CommonQueueSize"/> on address.</summary>
    private const ulong CommonQueueSelect = 0x16;

    /// <summary>queue_size: the selected queue's size, the device's maximum until written.</summary>
    private const ulong CommonQueueSize = 0x18;

    /// <summary>queue_msix_vector: the MSI-X entry that signals the selected queue.</summary>
    private const ulong CommonQueueMsixVector = 0x1A;

    /// <summary>queue_enable: 1 once the selected queue is ready.</summary>
    private const ulong CommonQueueEnable = 0x1C;

    /// <summary>queue_notify_off: the selected queue's doorbell offset, in units of the notify multiplier.</summary>
    private const ulong CommonQueueNotifyOff = 0x1E;

    /// <summary>queue_desc: physical address of the selected queue's descriptor table, 64 bits.</summary>
    private const ulong CommonQueueDesc = 0x20;

    /// <summary>queue_driver: physical address of the selected queue's available ring, 64 bits.</summary>
    private const ulong CommonQueueDriver = 0x28;

    /// <summary>queue_device: physical address of the selected queue's used ring, 64 bits.</summary>
    private const ulong CommonQueueDevice = 0x30;

    /// <summary>Bytes between the low and the high half of a 64-bit common register.</summary>
    private const ulong HighHalfOffset = 4;

    /// <summary>Bits in one half of a 64-bit register or feature word.</summary>
    private const int HalfWidthBits = 32;

    /// <summary>The first 32-bit half of the feature bits.</summary>
    private const uint FeatureSelectLow = 0;

    /// <summary>The second 32-bit half of the feature bits.</summary>
    private const uint FeatureSelectHigh = 1;

    /// <summary>The value queue_enable takes when the queue is ready.</summary>
    private const ushort QueueEnabled = 1;

    /// <summary>VIRTIO_MSI_NO_VECTOR: the device answers it when it refuses an entry.</summary>
    private const ushort NoVector = 0xFFFF;

    /// <summary>Bytes of one doorbell write: the queue index as a 16-bit value.</summary>
    private const ulong NotifyWriteBytes = 2;

    /// <summary>The cache value of a queue that was never activated: no doorbell write for it.</summary>
    private const ulong NotActivated = ulong.MaxValue;

    private readonly RegisterWindow _common;
    private readonly ulong _commonOffset;
    private readonly RegisterWindow _notify;
    private readonly ulong _notifyOffset;
    private readonly ulong _notifyLength;
    private readonly uint _notifyMultiplier;
    private readonly RegisterWindow _isr;
    private readonly ulong _isrOffset;
    private readonly RegisterWindow? _device;
    private readonly ulong _deviceOffset;
    private readonly ulong _deviceLength;
    private readonly ulong[] _notifyOffsets;
    private readonly VirtioDeviceType _deviceType;
    private volatile int _interruptEntryCount;

    /// <summary>
    /// Takes the windows the driver mapped and where each structure sits in
    /// them. Thread context, from the transport driver's probe.
    /// </summary>
    /// <param name="deviceType">The device type the driver derived from the function's ids.</param>
    /// <param name="common">The window over the BAR holding the common configuration.</param>
    /// <param name="commonOffset">Offset of the common configuration within <paramref name="common"/>.</param>
    /// <param name="notify">The window over the BAR holding the notification structure.</param>
    /// <param name="notifyOffset">Offset of the notification structure within <paramref name="notify"/>.</param>
    /// <param name="notifyLength">Bytes of the notification structure.</param>
    /// <param name="notifyMultiplier">notify_off_multiplier: bytes per unit of a queue's notify offset.</param>
    /// <param name="isr">The window over the BAR holding the ISR status byte.</param>
    /// <param name="isrOffset">Offset of the ISR status byte within <paramref name="isr"/>.</param>
    /// <param name="device">The window over the BAR holding the device configuration, or null without one.</param>
    /// <param name="deviceOffset">Offset of the device configuration within <paramref name="device"/>.</param>
    /// <param name="deviceLength">Bytes of the device configuration; 0 without one.</param>
    internal VirtioPciTransport(VirtioDeviceType deviceType, RegisterWindow common, ulong commonOffset, RegisterWindow notify, ulong notifyOffset, ulong notifyLength, uint notifyMultiplier, RegisterWindow isr, ulong isrOffset, RegisterWindow? device, ulong deviceOffset, ulong deviceLength)
    {
        _deviceType = deviceType;
        _common = common;
        _commonOffset = commonOffset;
        _notify = notify;
        _notifyOffset = notifyOffset;
        _notifyLength = notifyLength;
        _notifyMultiplier = notifyMultiplier;
        _isr = isr;
        _isrOffset = isrOffset;
        _device = device;
        _deviceOffset = deviceOffset;
        _deviceLength = deviceLength;
        _notifyOffsets = new ulong[VirtioAccess.MaxQueues];
        for (int i = 0; i < _notifyOffsets.Length; i++)
        {
            _notifyOffsets[i] = NotActivated;
        }
    }

    /// <inheritdoc/>
    public override VirtioDeviceType DeviceType => _deviceType;

    /// <summary>True: the PCI transport is a virtio 1.x transport with the FEATURES_OK step. Any context.</summary>
    public override bool SupportsFeaturesOk => true;

    /// <summary>
    /// How many MSI-X messages the driver connected, entry 0 to count - 1;
    /// 0 when none could be routed and the leaf polls. Set by the driver
    /// before the node is published. Any context.
    /// </summary>
    public override int InterruptEntryCount => _interruptEntryCount;

    /// <summary>Records how many message entries the driver connected. Thread context, before the node is published.</summary>
    /// <param name="count">The connected entries.</param>
    internal void SetInterruptEntryCount(int count) => _interruptEntryCount = count;

    /// <inheritdoc/>
    public override byte ReadStatus() => _common.Read8(_commonOffset + CommonDeviceStatus);

    /// <inheritdoc/>
    public override void WriteStatus(byte status) => _common.Write8(_commonOffset + CommonDeviceStatus, status);

    /// <inheritdoc/>
    public override ulong ReadDeviceFeatures()
    {
        _common.Write32(_commonOffset + CommonDeviceFeatureSelect, FeatureSelectLow);
        ulong features = _common.Read32(_commonOffset + CommonDeviceFeature);
        _common.Write32(_commonOffset + CommonDeviceFeatureSelect, FeatureSelectHigh);
        features |= (ulong)_common.Read32(_commonOffset + CommonDeviceFeature) << HalfWidthBits;
        return features;
    }

    /// <inheritdoc/>
    public override void WriteDriverFeatures(ulong features)
    {
        _common.Write32(_commonOffset + CommonDriverFeatureSelect, FeatureSelectLow);
        _common.Write32(_commonOffset + CommonDriverFeature, (uint)features);
        _common.Write32(_commonOffset + CommonDriverFeatureSelect, FeatureSelectHigh);
        _common.Write32(_commonOffset + CommonDriverFeature, (uint)(features >> HalfWidthBits));
    }

    /// <inheritdoc/>
    public override ushort ReadQueueMaxSize(ushort index)
    {
        SelectQueue(index);
        return _common.Read16(_commonOffset + CommonQueueSize);
    }

    /// <inheritdoc/>
    public override bool IsQueueReady(ushort index)
    {
        SelectQueue(index);
        return _common.Read16(_commonOffset + CommonQueueEnable) != 0;
    }

    /// <summary>
    /// Selects the queue, writes its size and the three ring addresses as
    /// low and high halves, reads its notify offset and refuses the queue
    /// before enabling it when that offset's doorbell does not fit the
    /// notify structure (virtio 4.1.4.4.1 sizes the structure for every
    /// queue's offset) or is not 2-byte aligned (virtio 4.1.4.4 requires an
    /// aligned cap.offset and a notify_off_multiplier of 0 or an even power
    /// of two; the window refuses a misaligned 16-bit write). A function
    /// that breaks either rule is refused here so the kit's queue creation
    /// fails cleanly and the leaf's DMA is unwound, instead of the window
    /// throwing on the first notify; otherwise the doorbell offset is cached
    /// and queue_enable is set. Thread context.
    /// </summary>
    /// <param name="index">The queue index, below <see cref="VirtioAccess.MaxQueues"/>.</param>
    /// <param name="layout">Where the kit placed the rings.</param>
    /// <returns>False when the index is at or above <see cref="VirtioAccess.MaxQueues"/>, or the doorbell does not fit the notify structure or is misaligned.</returns>
    public override bool ActivateQueue(ushort index, in VirtqueueLayout layout)
    {
        if (index >= VirtioAccess.MaxQueues)
        {
            return false;
        }

        SelectQueue(index);
        _common.Write16(_commonOffset + CommonQueueSize, layout.Size);
        WriteAddress(CommonQueueDesc, layout.DescriptorTable);
        WriteAddress(CommonQueueDriver, layout.AvailableRing);
        WriteAddress(CommonQueueDevice, layout.UsedRing);

        ushort notifyOff = _common.Read16(_commonOffset + CommonQueueNotifyOff);
        ulong doorbell = (ulong)notifyOff * _notifyMultiplier;
        ulong window = _notifyOffset + doorbell;
        if (doorbell + NotifyWriteBytes > _notifyLength || (window & (NotifyWriteBytes - 1)) != 0)
        {
            return false;
        }

        _notifyOffsets[index] = window;
        _common.Write16(_commonOffset + CommonQueueEnable, QueueEnabled);
        return true;
    }

    /// <summary>
    /// Writes the queue index at its cached doorbell. Nothing for a queue
    /// that was never activated. Interrupt context allowed; allocation-free.
    /// </summary>
    /// <param name="index">The queue index.</param>
    public override void NotifyQueue(ushort index)
    {
        if (index >= VirtioAccess.MaxQueues)
        {
            return;
        }

        ulong offset = _notifyOffsets[index];
        if (offset == NotActivated)
        {
            return;
        }

        _notify.Write16(offset, index);
    }

    /// <summary>
    /// With message entries connected, both bits: the device does not
    /// update the ISR byte for a message-signalled delivery. Without, the
    /// ISR byte, which is read-to-clear. Interrupt context; allocation-free.
    /// </summary>
    public override VirtioInterruptStatus ReadAndAcknowledgeInterrupt()
    {
        if (_interruptEntryCount > 0)
        {
            return VirtioInterruptStatus.Queue | VirtioInterruptStatus.Config;
        }

        return (VirtioInterruptStatus)_isr.Read8(_isrOffset);
    }

    /// <summary>Writes msix_config and reads it back. Thread context.</summary>
    /// <param name="entry">The entry, from 0.</param>
    /// <returns>False when the device answered NO_VECTOR.</returns>
    public override bool AssignConfigInterrupt(int entry)
    {
        _common.Write16(_commonOffset + CommonMsixConfig, (ushort)entry);
        return _common.Read16(_commonOffset + CommonMsixConfig) != NoVector;
    }

    /// <summary>Selects the queue, writes queue_msix_vector and reads it back. Thread context.</summary>
    /// <param name="index">The queue index.</param>
    /// <param name="entry">The entry, from 0.</param>
    /// <returns>False when the device answered NO_VECTOR.</returns>
    public override bool AssignQueueInterrupt(ushort index, int entry)
    {
        SelectQueue(index);
        _common.Write16(_commonOffset + CommonQueueMsixVector, (ushort)entry);
        return _common.Read16(_commonOffset + CommonQueueMsixVector) != NoVector;
    }

    /// <summary>Nothing: the PCI transport programs nothing after a reset. Thread context.</summary>
    public override void AfterReset()
    {
    }

    /// <summary>Reads one byte of the device configuration; 0 without a device configuration structure or past its end. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the device configuration.</param>
    public override byte ReadConfig8(uint offset) =>
        IsInDeviceConfig(offset, sizeof(byte)) ? _device!.Read8(_deviceOffset + offset) : (byte)0;

    /// <summary>Reads one word of the device configuration; 0 without a device configuration structure or past its end. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the device configuration, even.</param>
    public override ushort ReadConfig16(uint offset) =>
        IsInDeviceConfig(offset, sizeof(ushort)) ? _device!.Read16(_deviceOffset + offset) : (ushort)0;

    /// <summary>Reads one dword of the device configuration; 0 without a device configuration structure or past its end. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the device configuration, dword aligned.</param>
    public override uint ReadConfig32(uint offset) =>
        IsInDeviceConfig(offset, sizeof(uint)) ? _device!.Read32(_deviceOffset + offset) : 0;

    /// <summary>Writes one byte of the device configuration; dropped without a device configuration structure or past its end. Any context; allocation-free.</summary>
    /// <param name="offset">The offset within the device configuration.</param>
    /// <param name="value">The value.</param>
    public override void WriteConfig8(uint offset, byte value)
    {
        if (IsInDeviceConfig(offset, sizeof(byte)))
        {
            _device!.Write8(_deviceOffset + offset, value);
        }
    }

    /// <summary>True when the function has a device configuration structure and an access of <paramref name="size"/> bytes at <paramref name="offset"/> lies within it. Any context; allocation-free.</summary>
    private bool IsInDeviceConfig(uint offset, int size) =>
        _device is not null && offset < _deviceLength && _deviceLength - offset >= (ulong)size;

    /// <summary>Writes queue_select. Thread context.</summary>
    private void SelectQueue(ushort index) => _common.Write16(_commonOffset + CommonQueueSelect, index);

    /// <summary>Writes a 64-bit common register as its low and high halves. Thread context.</summary>
    private void WriteAddress(ulong register, ulong address)
    {
        _common.Write32(_commonOffset + register, (uint)address);
        _common.Write32(_commonOffset + register + HighHalfOffset, (uint)(address >> HalfWidthBits));
    }
}
