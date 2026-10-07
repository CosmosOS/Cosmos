// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Buses.Virtio;

/// <summary>
/// A match over virtio identities: one device type (specificity 1) or any
/// virtio device (specificity 0). The transport underneath is never part
/// of a match. Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class VirtioMatch : DeviceMatch
{
    private readonly VirtioDeviceType? _deviceType;

    /// <inheritdoc/>
    public override int Specificity => _deviceType is null ? 0 : 1;

    private VirtioMatch(VirtioDeviceType? deviceType)
    {
        _deviceType = deviceType;
    }

    /// <summary>Matches every virtio device of <paramref name="type"/>, whatever its transport.</summary>
    /// <param name="type">The device type.</param>
    public static VirtioMatch DeviceType(VirtioDeviceType type)
    {
        return new(type);
    }

    /// <summary>Matches every virtio device.</summary>
    public static VirtioMatch Any()
    {
        return new(null);
    }

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity)
    {
        return identity is VirtioIdentity virtio && (_deviceType is null || _deviceType.Value == virtio.DeviceType);
    }
}
