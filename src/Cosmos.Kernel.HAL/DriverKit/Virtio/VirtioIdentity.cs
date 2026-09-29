// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// The identity of a virtio device as its transport driver published it:
/// which transport carries it, the transport's own address for it, and the
/// device type. The path is <c>virtio:pci:0000:00:03.0</c> or
/// <c>virtio:mmio:a003e00</c>: the transport is visible in the path and
/// invisible to the leaf driver, which matches on the type alone. Built by
/// a transport driver in Cosmos.Kernel.Drivers, so the constructor is
/// public. Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class VirtioIdentity : DeviceIdentity
{
    /// <summary>Describes a virtio device.</summary>
    /// <param name="transport">The transport's name: "pci" or "mmio".</param>
    /// <param name="transportAddress">The transport's address for the device: a PCI function address, an MMIO slot base in hexadecimal.</param>
    /// <param name="deviceType">The device type the transport read.</param>
    /// <exception cref="ArgumentException"><paramref name="transport"/> or <paramref name="transportAddress"/> is null or empty.</exception>
    public VirtioIdentity(string transport, string transportAddress, VirtioDeviceType deviceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(transport);
        ArgumentException.ThrowIfNullOrEmpty(transportAddress);
        Transport = transport;
        TransportAddress = transportAddress;
        DeviceType = deviceType;
        Address = $"{transport}:{transportAddress}";
    }

    /// <summary>The transport's name: "pci" or "mmio".</summary>
    public string Transport { get; }

    /// <summary>The transport's address for the device, in the transport's notation.</summary>
    public string TransportAddress { get; }

    /// <summary>The device type.</summary>
    public VirtioDeviceType DeviceType { get; }

    /// <inheritdoc/>
    public override string BusName => "virtio";

    /// <inheritdoc/>
    public override string Address { get; }

    /// <inheritdoc/>
    public override string Describe() => $"type {(uint)DeviceType} ({TypeName(DeviceType)})";

    /// <summary>The type in words, or "unknown" for a type the kit has no name for.</summary>
    private static string TypeName(VirtioDeviceType type) => type switch
    {
        VirtioDeviceType.Network => "network",
        VirtioDeviceType.Block => "block",
        VirtioDeviceType.Console => "console",
        VirtioDeviceType.Entropy => "entropy",
        VirtioDeviceType.Balloon => "balloon",
        VirtioDeviceType.Scsi => "scsi",
        VirtioDeviceType.Gpu => "gpu",
        VirtioDeviceType.Input => "input",
        VirtioDeviceType.Socket => "socket",
        VirtioDeviceType.Crypto => "crypto",
        VirtioDeviceType.Sound => "sound",
        VirtioDeviceType.FileSystem => "filesystem",
        _ => "unknown",
    };
}
