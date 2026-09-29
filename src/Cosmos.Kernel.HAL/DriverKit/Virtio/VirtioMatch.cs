// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// A match over virtio identities: one device type (specificity 1) or any
/// virtio device (specificity 0). The transport underneath is never part
/// of a match. Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class VirtioMatch : DeviceMatch
{
    private readonly VirtioDeviceType? _deviceType;

    /// <summary>A match on <paramref name="deviceType"/>, or on any virtio device when null.</summary>
    /// <param name="deviceType">The device type, or null for any.</param>
    private VirtioMatch(VirtioDeviceType? deviceType)
    {
        _deviceType = deviceType;
    }

    /// <inheritdoc/>
    public override int Specificity => _deviceType is null ? 0 : 1;

    /// <summary>Matches every virtio device of <paramref name="type"/>, whatever its transport.</summary>
    /// <param name="type">The device type.</param>
    public static VirtioMatch DeviceType(VirtioDeviceType type) => new(type);

    /// <summary>Matches every virtio device.</summary>
    public static VirtioMatch Any() => new(null);

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) =>
        identity is VirtioIdentity virtio && (_deviceType is null || _deviceType.Value == virtio.DeviceType);
}
