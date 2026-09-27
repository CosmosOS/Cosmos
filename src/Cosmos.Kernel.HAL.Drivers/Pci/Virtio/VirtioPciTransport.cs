// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Rings;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio;

/// <summary>
/// A virtio device over PCI, modern interface only (virtio 1.2 §4.1): the
/// status handshake, feature negotiation, virtqueue setup, the doorbells and
/// the device-specific configuration, over the register windows the
/// function's vendor-specific capabilities point into.
/// </summary>
/// <remarks>
/// <para>
/// The legacy interface, where the same registers sit in an I/O BAR at fixed
/// offsets, is not supported: a device that offers only that has no vendor
/// capabilities, <see cref="TryCreate"/> finds nothing and the driver
/// declines it. Every virtio device QEMU and the cloud hypervisors present
/// offers the modern one.
/// </para>
/// <para>
/// Interrupts are the kit's: <c>TryRequestInterrupts</c> routes MSI-X entry 0
/// where the platform can, and polls the handler from the timer otherwise.
/// This class only tells the device which vector to raise, through
/// <see cref="TryUseMessageVector"/>. The ISR status byte is never read,
/// because the kit disables INTx on every function it binds, so the device
/// never asserts a line that would have to be acknowledged.
/// </para>
/// </remarks>
internal sealed class VirtioPciTransport
{
    /// <summary>PCI vendor ID every virtio device carries, Red Hat's.</summary>
    internal const ushort VirtioVendorId = 0x1AF4;

    /// <summary>VIRTIO_F_VERSION_1 (bit 32): the device speaks the modern specification.</summary>
    internal const ulong FeatureVersion1 = 1UL << 32;

    /// <summary>PCI capability ID of the vendor-specific capabilities virtio locates its registers with.</summary>
    private const byte VendorCapabilityId = 0x09;

    /// <summary>Config offset of the status register, whose bit 4 says a capability list is present.</summary>
    private const ushort StatusConfigOffset = 0x06;
    private const ushort StatusCapabilitiesList = 0x0010;

    /// <summary>Config offset of the capability list's head.</summary>
    private const ushort CapabilityPointerOffset = 0x34;

    /// <summary>A capability pointer's low two bits are reserved.</summary>
    private const byte CapabilityPointerMask = 0xFC;

    /// <summary>Bytes of configuration space the kit can reach, which bounds a capability's fields.</summary>
    private const int ConfigSpaceLength = 0x100;

    /// <summary>
    /// Entries to follow before giving up: the capability area spans 0x40 to
    /// 0xFF, four-byte aligned, so a list longer than this has a cycle.
    /// </summary>
    private const int MaxCapabilityEntries = 48;

    // virtio_pci_cap (virtio 1.2 §4.1.4): cap_vndr, cap_next, cap_len,
    // cfg_type, bar, id, padding, offset, length, and for the notify
    // capability notify_off_multiplier behind them.
    private const ushort CapabilityConfigType = 3;
    private const ushort CapabilityBar = 4;
    private const ushort CapabilityRegionOffset = 8;
    private const ushort CapabilityRegionLength = 12;
    private const ushort CapabilityNotifyMultiplier = 16;

    // virtio_pci_common_cfg (virtio 1.2 §4.1.4.3), offsets into the common
    // configuration window.
    private const ulong DeviceFeatureSelect = 0x00;
    private const ulong DeviceFeature = 0x04;
    private const ulong DriverFeatureSelect = 0x08;
    private const ulong DriverFeature = 0x0C;
    private const ulong ConfigMsixVector = 0x10;
    private const ulong DeviceStatus = 0x14;
    private const ulong QueueSelect = 0x16;
    private const ulong QueueSize = 0x18;
    private const ulong QueueMsixVector = 0x1A;
    private const ulong QueueEnable = 0x1C;
    private const ulong QueueNotifyOffset = 0x1E;
    private const ulong QueueDescriptorTable = 0x20;
    private const ulong QueueAvailableRing = 0x28;
    private const ulong QueueUsedRing = 0x30;

    /// <summary>The feature bits the driver selects from, the low half of what a device offers.</summary>
    private const int FeatureHalfShift = 32;

    /// <summary>VIRTIO_MSI_NO_VECTOR: no MSI-X vector, which is what a reset leaves behind.</summary>
    private const ushort NoVector = 0xFFFF;

    /// <summary>The kit routes MSI-X entry 0, the only entry it programs.</summary>
    private const ushort MessageVector = 0;

    /// <summary>Polls to give a reset before calling the device stuck, at ten microseconds each.</summary>
    private const int ResetPolls = 1000;

    private readonly PciDeviceContext _context;

    private readonly MmioRegion _common;
    private readonly ulong _commonOffset;

    private readonly MmioRegion _notify;
    private readonly ulong _notifyOffset;
    private readonly uint _notifyMultiplier;

    /// <summary>The device-specific configuration window, absent on a device that exposes none.</summary>
    private readonly MmioRegion? _deviceConfig;
    private readonly ulong _deviceConfigOffset;

    /// <summary>
    /// Each queue's doorbell, worked out as the queue was activated. Its
    /// length is the queue count the driver asked for, which bounds every
    /// index this transport accepts.
    /// </summary>
    private readonly ulong[] _doorbells;

    /// <summary>Set once the device has taken <see cref="MessageVector"/>, which makes the queues take it too.</summary>
    private bool _messageSignaled;

    private VirtioPciTransport(PciDeviceContext context, int queueCount, MmioRegion common, ulong commonOffset,
        MmioRegion notify, ulong notifyOffset, uint notifyMultiplier,
        MmioRegion? deviceConfig, ulong deviceConfigOffset)
    {
        _doorbells = new ulong[queueCount];
        _context = context;
        _common = common;
        _commonOffset = commonOffset;
        _notify = notify;
        _notifyOffset = notifyOffset;
        _notifyMultiplier = notifyMultiplier;
        _deviceConfig = deviceConfig;
        _deviceConfigOffset = deviceConfigOffset;
    }

    /// <summary>
    /// Finds the function's virtio capabilities and maps the windows they
    /// point at. Probe only, since it maps BARs.
    /// </summary>
    /// <param name="context">The binding attempt, whose function is a virtio device.</param>
    /// <param name="queueCount">
    /// Virtqueues the driver will set up, which sizes the doorbell table and
    /// bounds the index <see cref="TryCreateQueue"/> and <see cref="Notify"/>
    /// take. Each device type has its own count, and a driver that leaves a
    /// queue of its type unused leaves it out of this one.
    /// </param>
    /// <param name="transport">The transport when the call returns true.</param>
    /// <returns>
    /// False when the function offers no modern interface: no capability
    /// list, no common configuration or no notify capability, or a window
    /// that does not fit the BAR it names. The kit logged which.
    /// </returns>
    internal static bool TryCreate(PciDeviceContext context, int queueCount,
        [NotNullWhen(true)] out VirtioPciTransport? transport)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(queueCount);

        transport = null;
        if ((context.Function.ReadConfig16(StatusConfigOffset) & StatusCapabilitiesList) == 0)
        {
            context.WriteLog("the function has no capability list, so no modern virtio interface");
            return false;
        }

        MmioRegion? common = null;
        MmioRegion? notify = null;
        MmioRegion? deviceConfig = null;
        ulong commonOffset = 0;
        ulong notifyOffset = 0;
        ulong deviceConfigOffset = 0;
        uint notifyMultiplier = 0;

        // The capability list carries one vendor-specific entry per window,
        // so it is walked whole rather than asked for the first match.
        ushort capability = (ushort)(context.Function.ReadConfig8(CapabilityPointerOffset) & CapabilityPointerMask);
        for (int entry = 0; capability != 0 && entry < MaxCapabilityEntries; entry++)
        {
            if (context.Function.ReadConfig8(capability) == VendorCapabilityId
                && TryReadWindow(context, capability, out VirtioConfigType type, out MmioRegion? region, out ulong offset))
            {
                // The first capability of each type is the one to use.
                switch (type)
                {
                    case VirtioConfigType.Common when common is null:
                        common = region;
                        commonOffset = offset;
                        break;
                    case VirtioConfigType.Notify when notify is null:
                        notify = region;
                        notifyOffset = offset;
                        notifyMultiplier = ReadCapability32(context, capability, CapabilityNotifyMultiplier);
                        break;
                    case VirtioConfigType.Device when deviceConfig is null:
                        deviceConfig = region;
                        deviceConfigOffset = offset;
                        break;
                }
            }

            capability = (ushort)(context.Function.ReadConfig8((ushort)(capability + 1)) & CapabilityPointerMask);
        }

        if (common is null || notify is null)
        {
            context.WriteLog("the function has no common configuration or no notify window");
            return false;
        }

        transport = new VirtioPciTransport(context, queueCount, common, commonOffset, notify, notifyOffset,
            notifyMultiplier, deviceConfig, deviceConfigOffset);
        return true;
    }

    /// <summary>
    /// Resets the device and announces the driver: status back to zero, then
    /// ACKNOWLEDGE and DRIVER.
    /// </summary>
    /// <returns>False when the device never reported the reset as done.</returns>
    internal bool TryBegin()
    {
        WriteCommon8(DeviceStatus, (byte)VirtioDeviceStatus.Reset);

        // A reset is only over once the status reads back zero (virtio 1.2 §4.1.4.3.2).
        for (int polls = 0; ReadStatus() != VirtioDeviceStatus.Reset; polls++)
        {
            if (polls == ResetPolls)
            {
                _context.WriteLog("the device did not finish resetting");
                return false;
            }

            _context.Delay(TimeSpan.FromMicroseconds(10));
        }

        AddStatus(VirtioDeviceStatus.Acknowledge);
        AddStatus(VirtioDeviceStatus.Driver);
        return true;
    }

    /// <summary>
    /// Selects the features the driver wants of those the device offers, and
    /// asks the device to accept the selection.
    /// </summary>
    /// <param name="wanted">The bits to take where offered, VIRTIO_F_VERSION_1 among them.</param>
    /// <param name="negotiated">What both sides agreed on when the call returns true.</param>
    /// <returns>False when the device refused the selection, which leaves it unusable.</returns>
    internal bool TryNegotiateFeatures(ulong wanted, out ulong negotiated)
    {
        WriteCommon32(DeviceFeatureSelect, 0);
        ulong offered = ReadCommon32(DeviceFeature);
        WriteCommon32(DeviceFeatureSelect, 1);
        offered |= (ulong)ReadCommon32(DeviceFeature) << FeatureHalfShift;

        negotiated = offered & wanted;
        WriteCommon32(DriverFeatureSelect, 0);
        WriteCommon32(DriverFeature, (uint)negotiated);
        WriteCommon32(DriverFeatureSelect, 1);
        WriteCommon32(DriverFeature, (uint)(negotiated >> FeatureHalfShift));

        AddStatus(VirtioDeviceStatus.FeaturesOk);
        if ((ReadStatus() & VirtioDeviceStatus.FeaturesOk) == 0)
        {
            _context.WriteLog($"the device refused the features 0x{negotiated:X}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Points the device's configuration-change interrupt at the MSI-X entry
    /// the kit routes, and makes every queue activated afterwards raise it
    /// too. Called only when the kit granted MSI-X, and after
    /// <see cref="TryNegotiateFeatures"/>, since a reset clears the
    /// assignment.
    /// </summary>
    /// <returns>False when the device would not take the vector, which leaves the driver polling.</returns>
    internal bool TryUseMessageVector()
    {
        WriteCommon16(ConfigMsixVector, MessageVector);
        if (ReadCommon16(ConfigMsixVector) == NoVector)
        {
            _context.WriteLog("the device refused the configuration MSI-X vector");
            return false;
        }

        _messageSignaled = true;
        return true;
    }

    /// <summary>
    /// Builds queue <paramref name="index"/> and hands it to the device: its
    /// size is what the device allows, up to
    /// <paramref name="preferredCount"/>, its memory comes from the binding,
    /// and its doorbell is cached for <see cref="Notify"/>.
    /// </summary>
    /// <param name="index">The queue's index, as the device type numbers its queues.</param>
    /// <param name="preferredCount">Descriptors the driver would like, a power of two.</param>
    /// <param name="queue">The queue when the call returns true.</param>
    /// <returns>
    /// False when the device has no such queue, when it is already in use,
    /// when no DMA memory is free, or when the doorbell the device named lies
    /// outside the notify window.
    /// </returns>
    internal bool TryCreateQueue(ushort index, int preferredCount, [NotNullWhen(true)] out SplitVirtqueue? queue)
    {
        queue = null;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)index, _doorbells.Length);

        WriteCommon16(QueueSelect, index);
        if (ReadCommon16(QueueEnable) != 0)
        {
            _context.WriteLog($"queue {index} is already in use");
            return false;
        }

        int count = ReadCommon16(QueueSize);
        if (count == 0)
        {
            _context.WriteLog($"the device has no queue {index}");
            return false;
        }

        if (count > preferredCount)
        {
            count = preferredCount;
        }

        if (!_context.TryAllocateDma(SplitVirtqueue.MemoryLength(count), ulong.MaxValue, out DmaBuffer? memory))
        {
            _context.WriteLog($"no DMA memory for queue {index}");
            return false;
        }

        // The doorbell is at a fixed offset the device names once; a window
        // too small for it means the capabilities disagree with each other,
        // and ringing it would write past the mapping.
        ulong doorbell = _notifyOffset + (ReadCommon16(QueueNotifyOffset) * (ulong)_notifyMultiplier);
        if (doorbell + sizeof(ushort) > _notify.Length)
        {
            _context.WriteLog($"queue {index} has its doorbell outside the notify window");
            _context.FreeDma(memory);
            return false;
        }

        SplitVirtqueue created = new(memory, count);
        WriteCommon16(QueueSize, (ushort)count);
        WriteCommon64(QueueDescriptorTable, created.DescriptorTableAddress);
        WriteCommon64(QueueAvailableRing, created.AvailableRingAddress);
        WriteCommon64(QueueUsedRing, created.UsedRingAddress);

        if (_messageSignaled)
        {
            WriteCommon16(QueueMsixVector, MessageVector);
            if (ReadCommon16(QueueMsixVector) == NoVector)
            {
                _context.WriteLog($"the device refused queue {index}'s MSI-X vector");
            }
        }

        _doorbells[index] = doorbell;
        WriteCommon16(QueueEnable, 1);
        queue = created;
        return true;
    }

    /// <summary>Lets the device start, which is when it first looks at the queues.</summary>
    internal void Finish() => AddStatus(VirtioDeviceStatus.DriverOk);

    /// <summary>Tells the device the driver has given up on it, so it stops rather than waiting to be driven.</summary>
    internal void Fail() => AddStatus(VirtioDeviceStatus.Failed);

    /// <summary>
    /// Rings queue <paramref name="index"/>'s doorbell, which tells the
    /// device to look at what the driver has offered. Any context: one MMIO
    /// write, whose DMA barrier orders the ring stores in front of it.
    /// </summary>
    /// <param name="index">A queue <see cref="TryCreateQueue"/> activated.</param>
    internal void Notify(ushort index) => _notify.Write16(_doorbells[index], index);

    /// <summary>Reads a byte of the device-specific configuration, zero on a device that exposes none.</summary>
    /// <param name="offset">The offset in that window.</param>
    /// <returns>The byte.</returns>
    internal byte ReadDeviceConfig8(ulong offset) =>
        _deviceConfig is { } window ? window.Read8(_deviceConfigOffset + offset) : (byte)0;

    /// <summary>
    /// Writes a byte of the device-specific configuration, and does nothing
    /// on a device that exposes none. A few device types take a request in
    /// that window and answer in it, virtio-input's select and subselect
    /// among them (virtio 1.2 §5.8.4).
    /// </summary>
    /// <param name="offset">The offset in that window.</param>
    /// <param name="value">The byte to write.</param>
    internal void WriteDeviceConfig8(ulong offset, byte value) =>
        _deviceConfig?.Write8(_deviceConfigOffset + offset, value);

    /// <summary>Reads a 16-bit field of the device-specific configuration, zero on a device that exposes none.</summary>
    /// <param name="offset">The offset in that window, a multiple of 2.</param>
    /// <returns>The field.</returns>
    internal ushort ReadDeviceConfig16(ulong offset) =>
        _deviceConfig is { } window ? window.Read16(_deviceConfigOffset + offset) : (ushort)0;

    /// <summary>
    /// Reads one capability's window: which kind it is, the BAR it lies in,
    /// mapped, and its offset there.
    /// </summary>
    /// <returns>False when the capability names an unmappable BAR, or a window that runs past it.</returns>
    private static bool TryReadWindow(PciDeviceContext context, ushort capability, out VirtioConfigType type,
        [NotNullWhen(true)] out MmioRegion? region, out ulong offset)
    {
        type = (VirtioConfigType)context.Function.ReadConfig8((ushort)(capability + CapabilityConfigType));
        region = null;
        offset = 0;

        byte bar = context.Function.ReadConfig8((ushort)(capability + CapabilityBar));
        if (!context.TryMapBar(bar, out MmioRegion? mapped))
        {
            return false;
        }

        offset = ReadCapability32(context, capability, CapabilityRegionOffset);
        uint length = ReadCapability32(context, capability, CapabilityRegionLength);
        if (length == 0 || offset + length > mapped.Length)
        {
            context.WriteLog($"a virtio window at 0x{offset:X} for {length} bytes runs past BAR {bar}");
            return false;
        }

        region = mapped;
        return true;
    }

    /// <summary>
    /// Reads one 32-bit field of a capability, or zero when it would fall
    /// outside the configuration space, as it does for a capability that ends
    /// before the field.
    /// </summary>
    private static uint ReadCapability32(PciDeviceContext context, ushort capability, ushort field)
    {
        ushort offset = (ushort)(capability + field);
        return offset + sizeof(uint) <= ConfigSpaceLength ? context.Function.ReadConfig32(offset) : 0;
    }

    private VirtioDeviceStatus ReadStatus() => (VirtioDeviceStatus)ReadCommon8(DeviceStatus);

    private void AddStatus(VirtioDeviceStatus bits) => WriteCommon8(DeviceStatus, (byte)(ReadStatus() | bits));

    private byte ReadCommon8(ulong offset) => _common.Read8(_commonOffset + offset);

    private void WriteCommon8(ulong offset, byte value) => _common.Write8(_commonOffset + offset, value);

    private ushort ReadCommon16(ulong offset) => _common.Read16(_commonOffset + offset);

    private void WriteCommon16(ulong offset, ushort value) => _common.Write16(_commonOffset + offset, value);

    private uint ReadCommon32(ulong offset) => _common.Read32(_commonOffset + offset);

    private void WriteCommon32(ulong offset, uint value) => _common.Write32(_commonOffset + offset, value);

    private void WriteCommon64(ulong offset, ulong value) => _common.Write64(_commonOffset + offset, value);
}
