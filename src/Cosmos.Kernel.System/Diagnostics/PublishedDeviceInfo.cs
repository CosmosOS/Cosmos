// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Point-in-time snapshot of one device a driver published, produced by
/// <see cref="DriverInfo.TryGetDevice"/>. A withdrawn device leaves the
/// list, but the snapshot is taken without locking the kit, so a device being
/// withdrawn concurrently may still be listed for one job; <see cref="IsWithdrawn"/>
/// tells that case apart.
/// </summary>
public readonly struct PublishedDeviceInfo
{
    internal PublishedDeviceInfo(PublishedDeviceKind kind, string name, string? nodePath, string? driverName, bool isConsumed, bool isWithdrawn)
    {
        Kind = kind;
        Name = name;
        NodePath = nodePath;
        DriverName = driverName;
        IsConsumed = isConsumed;
        IsWithdrawn = isWithdrawn;
    }

    /// <summary>What kind of device this is.</summary>
    public PublishedDeviceKind Kind { get; }

    /// <summary>The name the driver gave the device.</summary>
    public string Name { get; }

    /// <summary>
    /// Path of the device node whose driver published the device, or
    /// <see langword="null"/> when no binding stands behind it (a device the
    /// firmware handed over).
    /// </summary>
    public string? NodePath { get; }

    /// <summary>
    /// Name of the driver that published the device, or <see langword="null"/>
    /// when no binding stands behind it.
    /// </summary>
    public string? DriverName { get; }

    /// <summary>
    /// Whether the kernel's manager for this kind of device received it. False
    /// when no consumer was installed for the kind at the time of publication,
    /// in which case the device's reports are discarded.
    /// </summary>
    public bool IsConsumed { get; }

    /// <summary>
    /// Whether the device has been withdrawn: true only for a device caught
    /// between its withdrawal and its removal from the list.
    /// </summary>
    public bool IsWithdrawn { get; }
}
