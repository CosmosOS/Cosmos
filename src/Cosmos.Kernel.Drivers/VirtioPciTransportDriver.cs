// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio PCI transport driver: binds every virtio function (vendor
/// 0x1AF4), locates the modern configuration structures through the
/// function's vendor capabilities, maps the BARs they point into, connects
/// the function's message interrupts and publishes one virtio child node
/// carrying a <see cref="VirtioAccess"/>, which a leaf driver such as
/// <see cref="VirtioNetDriver"/> binds without knowing the transport. A
/// leaf for another virtio type needs no change here. Deviations from a
/// full transport, by design: no legacy (I/O BAR) interface, and no INTx
/// fallback, since PCI lines are level and shared and the platform routing
/// is edge; a function whose messages cannot be routed (ARM64 without an
/// ITS, the virt machine's default GICv2) is published with no interrupt
/// entry and its leaf polls or declines. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Pci)]
public sealed class VirtioPciTransportDriver : Driver
{
    /// <summary>The PCI vendor id of every virtio function (Red Hat).</summary>
    private const ushort VirtioVendorId = 0x1AF4;

    /// <summary>A modern function's device id is this plus the virtio device type.</summary>
    private const ushort ModernDeviceIdBase = 0x1040;

    /// <summary>The first device id of a transitional function, whose type is its subsystem id.</summary>
    private const ushort TransitionalDeviceIdFirst = 0x1000;

    /// <summary>The last device id of a transitional function.</summary>
    private const ushort TransitionalDeviceIdLast = 0x103F;

    /// <summary>The vendor-specific capability id the virtio structures are located through.</summary>
    private const byte VendorCapabilityId = 0x09;

    /// <summary>Upper bound on the capability walk: the area 0x40..0xFF holds at most 48 dword-aligned entries.</summary>
    private const int MaxCapabilityEntries = 48;

    /// <summary>cfg_type within a virtio capability (virtio 4.1.4).</summary>
    private const byte CapabilityTypeOffset = 3;

    /// <summary>bar within a virtio capability: the BAR the structure sits in.</summary>
    private const byte CapabilityBarOffset = 4;

    /// <summary>offset within a virtio capability: where the structure starts in the BAR.</summary>
    private const byte CapabilityRegionOffsetOffset = 8;

    /// <summary>length within a virtio capability: the structure's bytes.</summary>
    private const byte CapabilityRegionLengthOffset = 12;

    /// <summary>notify_off_multiplier within the notify capability.</summary>
    private const byte CapabilityNotifyMultiplierOffset = 16;

    /// <summary>The alignment of a doorbell write, the bytes of a queue index: virtio 4.1.4.4 requires the notify structure's offset and a non-zero notify_off_multiplier to be multiples of it.</summary>
    private const uint NotifyAlignment = 2;

    /// <summary>cfg_type of the common configuration structure.</summary>
    private const byte ConfigTypeCommon = 1;

    /// <summary>cfg_type of the notification structure.</summary>
    private const byte ConfigTypeNotify = 2;

    /// <summary>cfg_type of the ISR status structure.</summary>
    private const byte ConfigTypeIsr = 3;

    /// <summary>cfg_type of the device-specific configuration structure.</summary>
    private const byte ConfigTypeDevice = 4;

    /// <summary>Base address registers of a type 0 header, the slots a capability may name.</summary>
    private const int BarCount = 6;

    /// <summary>The interrupt source index of the function's legacy line; the messages follow it.</summary>
    private const int LineInterruptIndex = 0;

    /// <summary>The most message entries the driver connects: the configuration change and one per queue the kit describes.</summary>
    private const int MaxEntries = 1 + VirtioAccess.MaxQueues;

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(vendorId: VirtioVendorId),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(VirtioPciTransportDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Derives the device type, locates the structures, maps the BARs,
    /// connects the messages and publishes the virtio node. Thread context
    /// on the kit worker; a declined or failed result makes the kit release
    /// everything acquired here and quiet the function again.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the child published; declined when the function is not one this driver carries.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The device type from the function's ids.
        PciIdentity identity = (PciIdentity)binding.Node.Identity;
        PciAccess pci = binding.Node.Access<PciAccess>();
        uint type;
        if (identity.DeviceId >= ModernDeviceIdBase)
        {
            type = (uint)(identity.DeviceId - ModernDeviceIdBase);
        }
        else if (identity.DeviceId >= TransitionalDeviceIdFirst && identity.DeviceId <= TransitionalDeviceIdLast)
        {
            type = identity.SubsystemId;
        }
        else
        {
            return ProbeResult.Declined("not a virtio function");
        }

        // 2. The capability walk: the first capability of each type is kept.
        CapabilityRegion common = default;
        CapabilityRegion notify = default;
        CapabilityRegion isr = default;
        CapabilityRegion device = default;
        uint notifyMultiplier = 0;
        byte capability = pci.FindCapability(VendorCapabilityId);
        for (int i = 0; capability != 0 && i < MaxCapabilityEntries; i++)
        {
            byte configType = pci.ReadConfig8((ushort)(capability + CapabilityTypeOffset));
            CapabilityRegion region = new(
                pci.ReadConfig8((ushort)(capability + CapabilityBarOffset)),
                pci.ReadConfig32((ushort)(capability + CapabilityRegionOffsetOffset)),
                pci.ReadConfig32((ushort)(capability + CapabilityRegionLengthOffset)));
            switch (configType)
            {
                case ConfigTypeCommon when !common.IsPresent:
                    common = region;
                    break;
                case ConfigTypeNotify when !notify.IsPresent:
                    notify = region;
                    notifyMultiplier = pci.ReadConfig32((ushort)(capability + CapabilityNotifyMultiplierOffset));
                    break;
                case ConfigTypeIsr when !isr.IsPresent:
                    isr = region;
                    break;
                case ConfigTypeDevice when !device.IsPresent:
                    device = region;
                    break;
            }

            capability = pci.FindCapability(VendorCapabilityId, capability);
        }

        if (!common.IsPresent || !notify.IsPresent || !isr.IsPresent)
        {
            return ProbeResult.Declined("no modern virtio capabilities (a legacy-only function)");
        }

        string? refusal = Validate(pci, in common, "common");
        refusal ??= Validate(pci, in notify, "notify");
        refusal ??= Validate(pci, in isr, "isr");
        if (refusal is null && device.IsPresent)
        {
            refusal = Validate(pci, in device, "device");
        }

        if (refusal is not null)
        {
            return ProbeResult.Declined(refusal);
        }

        if (notify.Offset % NotifyAlignment != 0 || (notifyMultiplier != 0 && notifyMultiplier % NotifyAlignment != 0))
        {
            return ProbeResult.Declined("the notify structure or notify_off_multiplier is not 2-byte aligned");
        }

        // 3. One window per distinct BAR, then decoding and DMA on.
        RegisterWindow?[] windows = new RegisterWindow?[BarCount];
        RegisterWindow commonWindow = WindowFor(binding, windows, common.Bar);
        RegisterWindow notifyWindow = WindowFor(binding, windows, notify.Bar);
        RegisterWindow isrWindow = WindowFor(binding, windows, isr.Bar);
        RegisterWindow? deviceWindow = device.IsPresent ? WindowFor(binding, windows, device.Bar) : null;
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 4. The transport, the access over it and the state.
        VirtioPciTransport transport = new(
            (VirtioDeviceType)type,
            commonWindow, common.Offset,
            notifyWindow, notify.Offset, notify.Length, notifyMultiplier,
            isrWindow, isr.Offset,
            deviceWindow, device.Offset, device.Length);
        VirtioAccess access = new(transport);
        VirtioPciEntryHandler[] handlers = new VirtioPciEntryHandler[MaxEntries];
        for (int i = 0; i < MaxEntries; i++)
        {
            handlers[i] = new VirtioPciEntryHandler(access, i);
        }

        VirtioPciTransportState state = new(access, transport, handlers);
        binding.DriverState = state;

        // 5. The messages, in order, until one cannot be routed.
        int wanted = Math.Min(pci.MessageInterruptCount, MaxEntries);
        int connected = 0;
        for (int i = 0; i < wanted && LineInterruptIndex + 1 + i < binding.Node.Interrupts.Count; i++)
        {
            if (!binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptIndex + 1 + i], handlers[i].OnInterrupt, out _))
            {
                break;
            }

            connected++;
        }

        transport.SetInterruptEntryCount(connected);

        // 6. The child node.
        VirtioIdentity childIdentity = new("pci", identity.Address, (VirtioDeviceType)type);
        state.Child = binding.PublishChild(childIdentity, [], access.InterruptsForPublish(), access);
        binding.Log($"virtio type {type}, {connected} message interrupts");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Turns bus mastering off once the children are torn down (each was
    /// reset by its own hooks). Thread context on the kit worker; nothing
    /// is written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (!reason.HardwarePresent)
        {
            return;
        }

        binding.Node.Access<PciAccess>().EnableBusMastering(false);
    }

    /// <summary>Checks that a structure's BAR exists, is an assigned memory window and holds the structure. Thread context.</summary>
    /// <returns>The reason to decline, or null when the structure is reachable.</returns>
    private static string? Validate(PciAccess pci, in CapabilityRegion region, string name)
    {
        if (region.Bar >= BarCount)
        {
            return $"the {name} structure names BAR {region.Bar}";
        }

        PciBar bar = pci.Bars[region.Bar];
        if (!bar.IsAssigned || bar.IsIo)
        {
            return $"the {name} structure sits in an unassigned or I/O BAR {region.Bar}";
        }

        if (region.Offset >= bar.Length || bar.Length - region.Offset < region.Length)
        {
            return $"the {name} structure does not fit BAR {region.Bar}";
        }

        return null;
    }

    /// <summary>Maps the BAR once and returns its window on every later call for the same BAR. Thread context.</summary>
    private static RegisterWindow WindowFor(DeviceBinding binding, RegisterWindow?[] windows, int bar)
    {
        RegisterWindow? window = windows[bar];
        if (window is null)
        {
            window = binding.MapRegisters(bar);
            windows[bar] = window;
        }

        return window;
    }

    /// <summary>Where one virtio structure sits, as its capability states it. A default instance stands for a capability not seen.</summary>
    private readonly struct CapabilityRegion
    {
        /// <summary>Records a capability's location.</summary>
        internal CapabilityRegion(byte bar, uint offset, uint length)
        {
            Bar = bar;
            Offset = offset;
            Length = length;
            IsPresent = true;
        }

        /// <summary>The BAR index the capability names.</summary>
        internal int Bar { get; }

        /// <summary>The structure's offset within the BAR.</summary>
        internal uint Offset { get; }

        /// <summary>The structure's length in bytes.</summary>
        internal uint Length { get; }

        /// <summary>True once a capability of this type was seen.</summary>
        internal bool IsPresent { get; }
    }
}
