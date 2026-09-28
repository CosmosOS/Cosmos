// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// The kind of a device a driver published, as reported by
/// <see cref="DriverInfo.TryGetDevice"/>. The set is closed: each kind is
/// one contract a driver implements and one consumer slot the kernel's
/// manager of that kind occupies.
/// </summary>
public enum PublishedDeviceKind : byte
{
    /// <summary>A keyboard.</summary>
    Keyboard,

    /// <summary>A mouse, touchpad or other pointing device.</summary>
    Pointer,

    /// <summary>A network interface.</summary>
    Network,

    /// <summary>A block device.</summary>
    Block,

    /// <summary>A display.</summary>
    Display,
}
