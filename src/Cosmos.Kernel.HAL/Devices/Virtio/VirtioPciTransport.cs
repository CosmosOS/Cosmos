// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Virtio PCI transport (virtio spec section 4.1), modern interface only: the
/// common/notify/ISR/device config regions are located through vendor-specific
/// PCI capabilities pointing into BARs. Interrupts use MSI-X through the
/// arch-neutral <see cref="MsiX"/>/MsiRouting path (LAPIC on x64, GICv3 ITS on
/// ARM64); without MSI-X the device runs in polled mode. Legacy-only
/// virtio-pci devices (I/O BAR interface, no vendor capabilities) are not
/// supported.
/// </summary>
internal sealed class VirtioPciTransport : VirtioTransport
{
    /// <summary>PCI vendor ID of all virtio devices (Red Hat).</summary>
    public const ushort VirtioVendorId = 0x1AF4;

    /// <summary>Modern virtio-pci device IDs are 0x1040 + device type.</summary>
    private const ushort ModernDeviceIdBase = 0x1040;
    /// <summary>Transitional virtio-pci device IDs occupy 0x1000..0x103F; the type is the subsystem ID.</summary>
    private const ushort TransitionalDeviceIdBase = 0x1000;
    /// <summary>Config-space offset of the subsystem device ID (Type-0 header).</summary>
    private const byte SubsystemIdOffset = 0x2E;

    /// <summary>PCI vendor-specific capability ID carrying virtio structure locations.</summary>
    private const byte VendorCapabilityId = 0x09;
    /// <summary>Status register bit 4 — Capabilities List present.</summary>
    private const ushort StatusCapabilitiesListMask = 0x0010;
    /// <summary>Upper bound on capability-list entries (cap area spans 0x40..0xFF, 4-byte aligned).</summary>
    private const int MaxCapabilityEntries = 48;
    /// <summary>Mask clearing the two reserved low bits of a capability pointer.</summary>
    private const byte CapabilityPointerMask = 0xFC;

    // virtio_pci_cap layout (spec section 4.1.4): cap_vndr @0, cap_next @1,
    // cap_len @2, cfg_type @3, bar @4, offset @8, length @12; the notify
    // capability additionally carries notify_off_multiplier @16.
    private const byte CapCfgTypeOffset = 3;
    private const byte CapBarOffset = 4;
    private const byte CapRegionOffsetOffset = 8;
    private const byte CapRegionLengthOffset = 12;
    private const byte CapNotifyMultiplierOffset = 16;

    // cfg_type values.
    private const byte CfgTypeCommon = 1;
    private const byte CfgTypeNotify = 2;
    private const byte CfgTypeIsr = 3;
    private const byte CfgTypeDevice = 4;

    // virtio_pci_common_cfg register offsets (spec section 4.1.4.3).
    private const uint CommonDeviceFeatureSelect = 0x00;
    private const uint CommonDeviceFeature = 0x04;
    private const uint CommonDriverFeatureSelect = 0x08;
    private const uint CommonDriverFeature = 0x0C;
    private const uint CommonMsixConfig = 0x10;
    private const uint CommonDeviceStatus = 0x14;
    private const uint CommonQueueSelect = 0x16;
    private const uint CommonQueueSize = 0x18;
    private const uint CommonQueueMsixVector = 0x1A;
    private const uint CommonQueueEnable = 0x1C;
    private const uint CommonQueueNotifyOff = 0x1E;
    private const uint CommonQueueDesc = 0x20;
    private const uint CommonQueueDriver = 0x28;
    private const uint CommonQueueDevice = 0x30;

    /// <summary>MSI-X vector value meaning "no vector" (VIRTIO_MSI_NO_VECTOR).</summary>
    private const ushort NoVector = 0xFFFF;

    /// <summary>Highest queue index the notify-address cache supports.</summary>
    private const int MaxQueues = 8;

    private readonly PciDevice _pci;
    private readonly uint _deviceType;
    private readonly ulong _commonCfg;
    private readonly ulong _notifyBase;
    private readonly uint _notifyOffMultiplier;
    private readonly ulong _isrStatus;
    private readonly ulong _deviceCfg;
    private readonly ulong[] _notifyAddresses = new ulong[MaxQueues];
    private MsiXContext _msix;
    private bool _msixActive;
    private VirtioInterruptHandler? _handler;

    public override uint DeviceType => _deviceType;
    public override string TransportName => "PCI";
    protected override bool SupportsFeaturesOk => true;

    /// <summary>
    /// True when MSI-X was enabled and a vector bound. False means the device
    /// has no usable MSI-X and therefore no interrupt source — drivers that
    /// need one refuse to start.
    /// </summary>
    public bool MsiXActive => _msixActive;

    /// <summary>The PCI function this transport drives.</summary>
    public PciDevice Pci => _pci;

    private VirtioPciTransport(PciDevice pci, uint deviceType, ulong commonCfg, ulong notifyBase,
        uint notifyOffMultiplier, ulong isrStatus, ulong deviceCfg)
    {
        _pci = pci;
        _deviceType = deviceType;
        _commonCfg = commonCfg;
        _notifyBase = notifyBase;
        _notifyOffMultiplier = notifyOffMultiplier;
        _isrStatus = isrStatus;
        _deviceCfg = deviceCfg;
    }

    /// <summary>
    /// Resolves the virtio device type of a PCI function, or 0 when the
    /// function is not a virtio device.
    /// </summary>
    public static uint GetDeviceType(PciDevice pci)
    {
        if (pci.VendorId != VirtioVendorId)
        {
            return 0;
        }

        if (pci.DeviceId >= ModernDeviceIdBase)
        {
            return (uint)(pci.DeviceId - ModernDeviceIdBase);
        }

        if (pci.DeviceId >= TransitionalDeviceIdBase)
        {
            // Transitional devices encode the type in the subsystem device ID.
            return pci.ReadRegister16(SubsystemIdOffset);
        }

        return 0;
    }

    /// <summary>
    /// Builds a transport for a virtio PCI function by locating its modern
    /// capability regions. Returns null when the device only offers the
    /// legacy interface or a required region is missing.
    /// </summary>
    public static VirtioPciTransport? TryCreate(PciDevice pci)
    {
        uint deviceType = GetDeviceType(pci);
        if (deviceType == 0)
        {
            return null;
        }

        if ((pci.ReadRegister16((byte)Config.Status) & StatusCapabilitiesListMask) == 0)
        {
            Serial.Write("[VirtioPci] Device has no capability list\n");
            return null;
        }

        ulong commonPhys = 0;
        ulong notifyPhys = 0;
        ulong isrPhys = 0;
        ulong devicePhys = 0;
        uint commonLen = 0;
        uint notifyLen = 0;
        uint isrLen = 0;
        uint deviceLen = 0;
        uint notifyMultiplier = 0;

        // FindCapability only returns the first match of a cap ID, and virtio
        // needs several vendor-specific (0x09) capabilities — walk the list.
        byte capOffset = (byte)(pci.ReadRegister8((byte)Config.CapabilityPointer) & CapabilityPointerMask);
        for (int i = 0; capOffset != 0 && i < MaxCapabilityEntries; i++)
        {
            byte capId = pci.ReadRegister8(capOffset);
            if (capId == VendorCapabilityId)
            {
                byte cfgType = pci.ReadRegister8((byte)(capOffset + CapCfgTypeOffset));
                byte bar = pci.ReadRegister8((byte)(capOffset + CapBarOffset));
                uint regionOffset = pci.ReadRegister32((byte)(capOffset + CapRegionOffsetOffset));
                uint regionLength = pci.ReadRegister32((byte)(capOffset + CapRegionLengthOffset));
                ulong barPhys = pci.GetBar64Address(bar);

                if (barPhys != 0)
                {
                    // The spec says to use the first instance of each cfg_type.
                    switch (cfgType)
                    {
                        case CfgTypeCommon when commonPhys == 0:
                            commonPhys = barPhys + regionOffset;
                            commonLen = regionLength;
                            break;
                        case CfgTypeNotify when notifyPhys == 0:
                            notifyPhys = barPhys + regionOffset;
                            notifyLen = regionLength;
                            notifyMultiplier = pci.ReadRegister32((byte)(capOffset + CapNotifyMultiplierOffset));
                            break;
                        case CfgTypeIsr when isrPhys == 0:
                            isrPhys = barPhys + regionOffset;
                            isrLen = regionLength;
                            break;
                        case CfgTypeDevice when devicePhys == 0:
                            devicePhys = barPhys + regionOffset;
                            deviceLen = regionLength;
                            break;
                    }
                }
            }

            capOffset = (byte)(pci.ReadRegister8((byte)(capOffset + 1)) & CapabilityPointerMask);
        }

        if (commonPhys == 0 || notifyPhys == 0 || isrPhys == 0)
        {
            Serial.Write("[VirtioPci] Missing modern virtio capabilities (legacy-only device?)\n");
            return null;
        }

        pci.EnableMemory(true);
        pci.EnableBusMaster(true);

        VirtioPciTransport transport = new VirtioPciTransport(
            pci,
            deviceType,
            MapRegion(commonPhys, commonLen),
            MapRegion(notifyPhys, notifyLen),
            notifyMultiplier,
            MapRegion(isrPhys, isrLen),
            devicePhys != 0 ? MapRegion(devicePhys, deviceLen) : 0);

        transport.TrySetupMsix();

        Serial.Write("[VirtioPci] Device type ");
        Serial.WriteNumber(deviceType);
        Serial.Write(" at ");
        Serial.WriteNumber(pci.Bus);
        Serial.Write(":");
        Serial.WriteNumber(pci.Slot);
        Serial.Write(".");
        Serial.WriteNumber(pci.Function);
        Serial.Write(transport._msixActive ? " (MSI-X)\n" : " (no MSI-X)\n");

        return transport;
    }

    /// <summary>
    /// Maps a BAR-relative region for MMIO access and returns its HHDM
    /// virtual address (same pattern as MsiX/NVMe: EnsureMmioMapped both ends,
    /// then access through the HHDM alias).
    /// </summary>
    private static ulong MapRegion(ulong phys, uint length)
    {
        PlatformHAL.Initializer?.EnsureMmioMapped(phys);
        if (length > 1)
        {
            PlatformHAL.Initializer?.EnsureMmioMapped(phys + length - 1);
        }

        return VirtioDma.PhysToVirt(phys);
    }

    private void TrySetupMsix()
    {
        MsiXContext? context = MsiX.Enable(_pci);
        if (context == null)
        {
            return;
        }

        _msix = context.Value;

        // One vector serves the whole device: entry 0 gets the config vector
        // and every queue vector. The drivers drain all rings on any signal.
        MsiX.SetEntry(_msix, 0, HandleMsiInterrupt);
        _msixActive = true;
    }

    protected override void AfterReset()
    {
        // Device reset clears the MSI-X vector assignments; restore the
        // config-change vector on every (re-)initialization.
        if (_msixActive)
        {
            WriteCommon16(CommonMsixConfig, 0);
            if (ReadCommon16(CommonMsixConfig) == NoVector)
            {
                Serial.Write("[VirtioPci] Device refused config MSI-X vector\n");
            }
        }
    }

    protected override byte GetStatus() => Native.MMIO.Read8(_commonCfg + CommonDeviceStatus);

    protected override void SetStatus(byte status) => Native.MMIO.Write8(_commonCfg + CommonDeviceStatus, status);

    protected override ulong ReadDeviceFeatures()
    {
        WriteCommon32(CommonDeviceFeatureSelect, 0);
        ulong features = ReadCommon32(CommonDeviceFeature);
        WriteCommon32(CommonDeviceFeatureSelect, 1);
        features |= (ulong)ReadCommon32(CommonDeviceFeature) << 32;
        return features;
    }

    protected override void WriteDriverFeatures(ulong features)
    {
        WriteCommon32(CommonDriverFeatureSelect, 0);
        WriteCommon32(CommonDriverFeature, (uint)features);
        WriteCommon32(CommonDriverFeatureSelect, 1);
        WriteCommon32(CommonDriverFeature, (uint)(features >> 32));
    }

    protected override bool IsQueueInUse(ushort index)
    {
        WriteCommon16(CommonQueueSelect, index);
        return ReadCommon16(CommonQueueEnable) != 0;
    }

    protected override uint GetQueueMaxSize(ushort index)
    {
        WriteCommon16(CommonQueueSelect, index);
        return ReadCommon16(CommonQueueSize);
    }

    protected override bool ActivateQueue(ushort index, Virtqueue queue)
    {
        if (index >= MaxQueues)
        {
            Serial.Write("[VirtioPci] Queue index out of range\n");
            return false;
        }

        WriteCommon16(CommonQueueSelect, index);
        WriteCommon16(CommonQueueSize, (ushort)queue.QueueSize);

        if (_msixActive)
        {
            WriteCommon16(CommonQueueMsixVector, 0);
            if (ReadCommon16(CommonQueueMsixVector) == NoVector)
            {
                Serial.Write("[VirtioPci] Device refused queue MSI-X vector\n");
            }
        }

        WriteCommon32(CommonQueueDesc, (uint)queue.DescriptorTableAddr);
        WriteCommon32(CommonQueueDesc + 4, (uint)(queue.DescriptorTableAddr >> 32));
        WriteCommon32(CommonQueueDriver, (uint)queue.AvailableRingAddr);
        WriteCommon32(CommonQueueDriver + 4, (uint)(queue.AvailableRingAddr >> 32));
        WriteCommon32(CommonQueueDevice, (uint)queue.UsedRingAddr);
        WriteCommon32(CommonQueueDevice + 4, (uint)(queue.UsedRingAddr >> 32));

        // The per-queue doorbell address is static — cache it for NotifyQueue.
        ushort notifyOff = ReadCommon16(CommonQueueNotifyOff);
        _notifyAddresses[index] = _notifyBase + (ulong)notifyOff * _notifyOffMultiplier;

        WriteCommon16(CommonQueueEnable, 1);
        return true;
    }

    public override void NotifyQueue(ushort index)
    {
        // Ring/descriptor writes are Normal memory, the doorbell is Device
        // memory — order them before the store the device acts on.
        PlatformHAL.Initializer?.DmaBarrier();
        Native.MMIO.Write16(_notifyAddresses[index], index);
    }

    public override uint ReadAndAckIsr()
    {
        // The ISR status byte is read-to-clear and deasserts INTx. With MSI-X
        // active the device does not update it and this reads 0.
        return Native.MMIO.Read8(_isrStatus);
    }

    public override bool EnableInterrupt(VirtioInterruptHandler handler)
    {
        _handler = handler;
        if (_msixActive)
        {
            return true;
        }

        // No INTx fallback: PCI INTx lines are level-low and shared, while
        // the IOAPIC line routing available here programs edge/active-high
        // and installing a handler would clobber whichever driver (e.g.
        // E1000E) already owns the shared line. Every stock virtio-pci
        // device exposes MSI-X; without it the device runs in polled mode.
        Serial.Write("[VirtioPci] No MSI-X; device runs in polled mode\n");
        return false;
    }

    private void HandleMsiInterrupt(ref IRQContext context)
    {
        // MSI-X delivery is edge-style and per-device; the ISR register is
        // not used. Report a queue notification — drivers drain their rings.
        _handler?.Invoke(IsrQueue);
    }

    public override byte ReadDeviceConfig8(uint offset) =>
        _deviceCfg != 0 ? Native.MMIO.Read8(_deviceCfg + offset) : (byte)0;

    public override ushort ReadDeviceConfig16(uint offset) =>
        _deviceCfg != 0 ? Native.MMIO.Read16(_deviceCfg + offset) : (ushort)0;

    public override void WriteDeviceConfig8(uint offset, byte value)
    {
        if (_deviceCfg != 0)
        {
            Native.MMIO.Write8(_deviceCfg + offset, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint ReadCommon32(uint offset) => Native.MMIO.Read32(_commonCfg + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteCommon32(uint offset, uint value) => Native.MMIO.Write32(_commonCfg + offset, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ReadCommon16(uint offset) => Native.MMIO.Read16(_commonCfg + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteCommon16(uint offset, ushort value) => Native.MMIO.Write16(_commonCfg + offset, value);
}
