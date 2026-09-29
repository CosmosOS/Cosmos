// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Platform;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The virtio MMIO transport driver: binds a platform node compatible with
/// "virtio,mmio" (a slot of the virt machine's window on ARM64), validates
/// the slot's magic, device id and version, connects the slot's line when
/// the platform routes it and publishes one virtio child node carrying a
/// <see cref="VirtioAccess"/>, which a leaf driver binds without knowing
/// the transport. An empty or foreign slot is declined; the machine
/// description lists what it presents and the driver still checks.
/// <see cref="Probe"/> runs in thread context on the kit worker; there is
/// no detach work, since the child's hooks reset the device and the kit
/// invalidates the window.
/// </summary>
[Driver]
public sealed class VirtioMmioTransportDriver : Driver
{
    /// <summary>The compatible string the machine description gives a virtio-mmio slot.</summary>
    private const string CompatibleString = "virtio,mmio";

    /// <summary>The resource index of the slot's register window.</summary>
    private const int WindowResourceIndex = 0;

    /// <summary>The interrupt source index of the slot's line.</summary>
    private const int LineInterruptIndex = 0;

    /// <summary>Bytes a slot's window spans: the registers through the start of the configuration space.</summary>
    private const ulong MinimumWindowBytes = 0x200;

    private readonly DeviceMatch[] _matches =
    [
        PlatformMatch.Compatible(CompatibleString),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(VirtioMmioTransportDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Maps the slot, checks it holds a virtio device, connects the line
    /// and publishes the virtio node. Thread context on the kit worker; a
    /// declined result makes the kit release the window again.
    /// </summary>
    /// <param name="binding">The slot's node and the kit facilities for it.</param>
    /// <returns>Bound with the child published; declined when the slot holds no virtio device this driver can carry.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The window.
        if (binding.Node.Resources.Count == 0)
        {
            return ProbeResult.Declined("no register window");
        }

        RegisterWindow window = binding.MapRegisters(WindowResourceIndex);
        if (window.Length < MinimumWindowBytes)
        {
            return ProbeResult.Declined($"the register window spans {window.Length} bytes, less than {MinimumWindowBytes}");
        }

        // 2. What the slot holds.
        if (window.Read32(VirtioMmioTransport.MagicValue) != VirtioMmioTransport.Magic)
        {
            return ProbeResult.Declined("no virtio device in the slot");
        }

        uint type = window.Read32(VirtioMmioTransport.DeviceId);
        if (type == 0)
        {
            return ProbeResult.Declined("empty slot");
        }

        uint version = window.Read32(VirtioMmioTransport.Version);
        if (version != VirtioMmioTransport.LegacyVersion && version != VirtioMmioTransport.ModernVersion)
        {
            return ProbeResult.Declined($"unknown virtio-mmio version {version}");
        }

        // 3. The transport, the access over it and the state.
        VirtioMmioTransport transport = new(window, version, (VirtioDeviceType)type);
        VirtioAccess access = new(transport);
        VirtioMmioTransportState state = new(access, transport);
        binding.DriverState = state;

        // 4. The line, when the platform routes it.
        bool hasLine = false;
        if (binding.Node.Interrupts.Count > LineInterruptIndex)
        {
            hasLine = binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptIndex], state.OnInterrupt, out _);
        }

        transport.SetInterruptEntryCount(hasLine ? 1 : 0);

        // 5. The child node.
        string address = $"{binding.Node.Resources[WindowResourceIndex].PhysicalBase:x}";
        VirtioIdentity childIdentity = new("mmio", address, (VirtioDeviceType)type);
        state.Child = binding.PublishChild(childIdentity, [], access.InterruptsForPublish(), access);
        binding.Log($"virtio type {type}, version {version}, {(hasLine ? "line" : "no line")}");
        return ProbeResult.Bound;
    }
}
