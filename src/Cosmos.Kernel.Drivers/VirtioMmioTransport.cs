// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio MMIO transport (virtio specification section 4.2): one
/// register window per slot, in the legacy (version 1) or the modern
/// (version 2) register flow, reached through the window
/// <see cref="VirtioMmioTransportDriver"/> mapped. The kit's
/// <see cref="VirtioAccess"/> calls the members; the driver only sets
/// <see cref="InterruptEntryCount"/> before it publishes the node. The
/// execution context of each member is in its summary.
/// </summary>
public sealed class VirtioMmioTransport : VirtioTransport
{
    /// <summary>MagicValue: reads "virt" in little endian on a virtio slot.</summary>
    internal const ulong MagicValue = 0x000;

    /// <summary>Version: 1 for the legacy interface, 2 for virtio 1.x.</summary>
    internal const ulong Version = 0x004;

    /// <summary>DeviceID: the virtio device type; 0 on an empty slot.</summary>
    internal const ulong DeviceId = 0x008;

    /// <summary>DeviceFeatures: the selected half of the device's feature bits.</summary>
    private const ulong DeviceFeatures = 0x010;

    /// <summary>DeviceFeaturesSel: which half <see cref="DeviceFeatures"/> shows.</summary>
    private const ulong DeviceFeaturesSel = 0x014;

    /// <summary>DriverFeatures: the selected half of the driver's accepted feature bits.</summary>
    private const ulong DriverFeatures = 0x020;

    /// <summary>DriverFeaturesSel: which half <see cref="DriverFeatures"/> writes.</summary>
    private const ulong DriverFeaturesSel = 0x024;

    /// <summary>GuestPageSize (legacy): the unit of QueuePFN, written after a reset.</summary>
    private const ulong GuestPageSize = 0x028;

    /// <summary>QueueSel: which queue the registers from <see cref="QueueNumMax"/> on address.</summary>
    private const ulong QueueSel = 0x030;

    /// <summary>QueueNumMax: the largest size the device supports for the selected queue.</summary>
    private const ulong QueueNumMax = 0x034;

    /// <summary>QueueNum: the size the driver chose for the selected queue.</summary>
    private const ulong QueueNum = 0x038;

    /// <summary>QueueAlign (legacy): the used ring's alignment.</summary>
    private const ulong QueueAlign = 0x03C;

    /// <summary>QueuePFN (legacy): the page frame number of the selected queue's block; 0 when not in use.</summary>
    private const ulong QueuePfn = 0x040;

    /// <summary>QueueReady (modern): 1 once the selected queue is ready.</summary>
    private const ulong QueueReady = 0x044;

    /// <summary>QueueNotify: the doorbell, written with the queue index.</summary>
    private const ulong QueueNotify = 0x050;

    /// <summary>InterruptStatus: the pending interrupt bits.</summary>
    private const ulong InterruptStatus = 0x060;

    /// <summary>InterruptACK: written with the bits handled.</summary>
    private const ulong InterruptAck = 0x064;

    /// <summary>Status: the device status register.</summary>
    private const ulong Status = 0x070;

    /// <summary>QueueDescLow (modern): low half of the selected queue's descriptor table address.</summary>
    private const ulong QueueDescLow = 0x080;

    /// <summary>QueueDescHigh (modern): high half of the selected queue's descriptor table address.</summary>
    private const ulong QueueDescHigh = 0x084;

    /// <summary>QueueDriverLow (modern): low half of the selected queue's available ring address.</summary>
    private const ulong QueueDriverLow = 0x090;

    /// <summary>QueueDriverHigh (modern): high half of the selected queue's available ring address.</summary>
    private const ulong QueueDriverHigh = 0x094;

    /// <summary>QueueDeviceLow (modern): low half of the selected queue's used ring address.</summary>
    private const ulong QueueDeviceLow = 0x0A0;

    /// <summary>QueueDeviceHigh (modern): high half of the selected queue's used ring address.</summary>
    private const ulong QueueDeviceHigh = 0x0A4;

    /// <summary>Config: the first byte of the device-specific configuration space.</summary>
    private const ulong Config = 0x100;

    /// <summary>The value MagicValue reads on a virtio slot.</summary>
    internal const uint Magic = 0x74726976;

    /// <summary>The legacy interface's version.</summary>
    internal const uint LegacyVersion = 1;

    /// <summary>The virtio 1.x interface's version.</summary>
    internal const uint ModernVersion = 2;

    /// <summary>The guest page size written after a reset on the legacy interface, and the unit of QueuePFN.</summary>
    internal const uint LegacyGuestPageSize = 4096;

    /// <summary>Bits in one half of the feature bits.</summary>
    private const int HalfWidthBits = 32;

    /// <summary>The first 32-bit half of the feature bits.</summary>
    private const uint FeatureSelectLow = 0;

    /// <summary>The second 32-bit half of the feature bits, present from version 2.</summary>
    private const uint FeatureSelectHigh = 1;

    /// <summary>The value QueueReady takes when the queue is ready.</summary>
    private const uint Ready = 1;

    private readonly RegisterWindow _window;
    private readonly uint _version;
    private readonly VirtioDeviceType _deviceType;
    private volatile int _interruptEntryCount;

    /// <summary>Takes the slot's window and what the driver read from it. Thread context, from the transport driver's probe.</summary>
    /// <param name="window">The window over the slot's registers.</param>
    /// <param name="version">The interface version, <see cref="LegacyVersion"/> or <see cref="ModernVersion"/>.</param>
    /// <param name="deviceType">The device type read from DeviceID.</param>
    internal VirtioMmioTransport(RegisterWindow window, uint version, VirtioDeviceType deviceType)
    {
        _window = window;
        _version = version;
        _deviceType = deviceType;
    }

    /// <inheritdoc/>
    public override VirtioDeviceType DeviceType => _deviceType;

    /// <summary>True from version 2 on; the legacy interface has no FEATURES_OK step. Any context.</summary>
    public override bool SupportsFeaturesOk => _version >= ModernVersion;

    /// <summary>1 when the driver connected the slot's line, else 0. Set by the driver before the node is published. Any context.</summary>
    public override int InterruptEntryCount => _interruptEntryCount;

    /// <summary>Records whether the driver connected the line. Thread context, before the node is published.</summary>
    /// <param name="count">1 with the line, 0 without.</param>
    internal void SetInterruptEntryCount(int count) => _interruptEntryCount = count;

    /// <inheritdoc/>
    public override byte ReadStatus() => (byte)_window.Read32(Status);

    /// <inheritdoc/>
    public override void WriteStatus(byte status) => _window.Write32(Status, status);

    /// <summary>Reads the low half always and the high half from version 2 on. Thread context.</summary>
    public override ulong ReadDeviceFeatures()
    {
        _window.Write32(DeviceFeaturesSel, FeatureSelectLow);
        ulong features = _window.Read32(DeviceFeatures);
        if (_version >= ModernVersion)
        {
            _window.Write32(DeviceFeaturesSel, FeatureSelectHigh);
            features |= (ulong)_window.Read32(DeviceFeatures) << HalfWidthBits;
        }

        return features;
    }

    /// <summary>Writes the low half always and the high half from version 2 on. Thread context.</summary>
    /// <param name="features">The features the driver accepts.</param>
    public override void WriteDriverFeatures(ulong features)
    {
        _window.Write32(DriverFeaturesSel, FeatureSelectLow);
        _window.Write32(DriverFeatures, (uint)features);
        if (_version >= ModernVersion)
        {
            _window.Write32(DriverFeaturesSel, FeatureSelectHigh);
            _window.Write32(DriverFeatures, (uint)(features >> HalfWidthBits));
        }
    }

    /// <inheritdoc/>
    public override ushort ReadQueueMaxSize(ushort index)
    {
        _window.Write32(QueueSel, index);
        return (ushort)_window.Read32(QueueNumMax);
    }

    /// <summary>Legacy: QueuePFN non-zero; modern: QueueReady non-zero. Thread context.</summary>
    /// <param name="index">The queue index.</param>
    public override bool IsQueueReady(ushort index)
    {
        _window.Write32(QueueSel, index);
        return _version == LegacyVersion
            ? _window.Read32(QueuePfn) != 0
            : _window.Read32(QueueReady) != 0;
    }

    /// <summary>
    /// Selects the queue and writes its size. Legacy: refuses a layout whose
    /// alignment is not <see cref="LegacyGuestPageSize"/> or whose block is
    /// not aligned to it (the kit lays the used ring on a page boundary and
    /// allocates the block page-aligned, so both hold whenever the kit's
    /// page size is 4096; the refusal surfaces as a failed queue creation),
    /// else writes QueueAlign and QueuePFN. Modern: the three address pairs,
    /// then QueueReady. Thread context.
    /// </summary>
    /// <param name="index">The queue index.</param>
    /// <param name="layout">Where the kit placed the rings.</param>
    /// <returns>False when the legacy interface cannot express the layout.</returns>
    public override bool ActivateQueue(ushort index, in VirtqueueLayout layout)
    {
        _window.Write32(QueueSel, index);
        _window.Write32(QueueNum, layout.Size);
        if (_version == LegacyVersion)
        {
            if (layout.Alignment != LegacyGuestPageSize || layout.Base % LegacyGuestPageSize != 0)
            {
                return false;
            }

            _window.Write32(QueueAlign, layout.Alignment);
            _window.Write32(QueuePfn, (uint)(layout.Base / LegacyGuestPageSize));
            return true;
        }

        _window.Write32(QueueDescLow, (uint)layout.DescriptorTable);
        _window.Write32(QueueDescHigh, (uint)(layout.DescriptorTable >> HalfWidthBits));
        _window.Write32(QueueDriverLow, (uint)layout.AvailableRing);
        _window.Write32(QueueDriverHigh, (uint)(layout.AvailableRing >> HalfWidthBits));
        _window.Write32(QueueDeviceLow, (uint)layout.UsedRing);
        _window.Write32(QueueDeviceHigh, (uint)(layout.UsedRing >> HalfWidthBits));
        _window.Write32(QueueReady, Ready);
        return true;
    }

    /// <summary>Writes the queue index to QueueNotify. Interrupt context allowed; allocation-free.</summary>
    /// <param name="index">The queue index.</param>
    public override void NotifyQueue(ushort index) => _window.Write32(QueueNotify, index);

    /// <summary>Reads InterruptStatus and, when non-zero, acknowledges it through InterruptACK: the line is level-triggered. Interrupt context; allocation-free.</summary>
    public override VirtioInterruptStatus ReadAndAcknowledgeInterrupt()
    {
        uint status = _window.Read32(InterruptStatus);
        if (status != 0)
        {
            _window.Write32(InterruptAck, status);
        }

        return (VirtioInterruptStatus)status;
    }

    /// <summary>True: the one line signals everything, so there is nothing to assign. Thread context.</summary>
    /// <param name="entry">The entry, unused.</param>
    public override bool AssignConfigInterrupt(int entry) => true;

    /// <summary>True: the one line signals everything, so there is nothing to assign. Thread context.</summary>
    /// <param name="index">The queue index, unused.</param>
    /// <param name="entry">The entry, unused.</param>
    public override bool AssignQueueInterrupt(ushort index, int entry) => true;

    /// <summary>Legacy: writes GuestPageSize, the unit of every QueuePFN that follows. Modern: nothing. Thread context.</summary>
    public override void AfterReset()
    {
        if (_version == LegacyVersion)
        {
            _window.Write32(GuestPageSize, LegacyGuestPageSize);
        }
    }

    /// <inheritdoc/>
    public override byte ReadConfig8(uint offset) => _window.Read8(Config + offset);

    /// <inheritdoc/>
    public override ushort ReadConfig16(uint offset) => _window.Read16(Config + offset);

    /// <inheritdoc/>
    public override uint ReadConfig32(uint offset) => _window.Read32(Config + offset);

    /// <inheritdoc/>
    public override void WriteConfig8(uint offset, byte value) => _window.Write8(Config + offset, value);
}
