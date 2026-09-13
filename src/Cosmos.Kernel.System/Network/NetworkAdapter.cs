// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// A handle to one of the network devices HAL enumeration registered. Obtained
/// from <see cref="NetworkManager.GetAdapter(int)"/> or
/// <see cref="NetworkManager.Primary"/>, and handed back to the ring to inspect
/// that device or to configure it with
/// <see cref="Config.IPConfig.Enable(NetworkAdapter, IPv4.Address, IPv4.Address, IPv4.Address)"/>.
/// </summary>
/// <remarks>
/// The handle carries the device's registration index biased by one, so a
/// default-constructed value names no device rather than silently naming the
/// first one.
/// </remarks>
public readonly struct NetworkAdapter : IEquatable<NetworkAdapter>
{
    // One past the registration index. Slot 0 is the default-constructed
    // handle and belongs to no device.
    private readonly int _slot;

    internal NetworkAdapter(int index)
    {
        _slot = index + 1;
    }

    /// <summary>
    /// The device's registration index, or -1 for a handle that names none.
    /// </summary>
    public int Index => _slot - 1;

    /// <summary>
    /// Whether this handle still names a registered device. False for a
    /// default-constructed handle.
    /// </summary>
    public bool IsValid => _slot > 0 && _slot <= NetworkManager.DeviceCount;

    internal INetworkDevice? Device => IsValid ? NetworkManager.GetDevice(Index) : null;

    /// <summary>
    /// The device's name, or null when the handle names no device.
    /// </summary>
    public string? Name => Device?.Name;

    /// <summary>
    /// The device's MAC address, or null when the handle names no device. A
    /// device that has not finished initializing reports
    /// <see cref="MACAddress.None"/>, the all-zero address, rather than null:
    /// the two sentinels are different questions, and <see cref="Ready"/>
    /// answers the second one.
    /// </summary>
    public MACAddress? MacAddress => Device?.MacAddress;

    /// <summary>
    /// Whether the device reports its link up.
    /// </summary>
    public bool LinkUp => Device?.LinkUp ?? false;

    /// <summary>
    /// Whether the device finished initializing and can carry traffic.
    /// </summary>
    public bool Ready => Device?.Ready ?? false;

    /// <summary>
    /// The IPv4 configuration in force on this device, or null when the device
    /// is unconfigured or the handle names none. Assign one with
    /// <see cref="Config.IPConfig.Enable(NetworkAdapter, IPv4.Address, IPv4.Address, IPv4.Address)"/>.
    /// </summary>
    public Config.IPConfig? IPConfig
    {
        get
        {
            INetworkDevice? device = Device;
            return device is null ? null : Config.IPConfig.Get(device);
        }
    }

    /// <summary>
    /// Whether two handles name the same registered device.
    /// </summary>
    /// <param name="other">The handle to compare against.</param>
    /// <returns>True when both name the same device, or both name none.</returns>
    public bool Equals(NetworkAdapter other)
    {
        return _slot == other._slot;
    }

    /// <summary>
    /// Whether <paramref name="obj"/> is a handle naming the same device.
    /// </summary>
    /// <param name="obj">Object to compare against.</param>
    /// <returns>True when it is a <see cref="NetworkAdapter"/> naming the same device.</returns>
    public override bool Equals(object? obj)
    {
        return obj is NetworkAdapter other && Equals(other);
    }

    /// <summary>
    /// Get a hash code derived from the registration index.
    /// </summary>
    /// <returns>Hash code for this handle.</returns>
    public override int GetHashCode()
    {
        return _slot;
    }

    /// <summary>
    /// Whether two handles name the same registered device.
    /// </summary>
    /// <param name="left">First handle.</param>
    /// <param name="right">Second handle.</param>
    /// <returns>True when both name the same device.</returns>
    public static bool operator ==(NetworkAdapter left, NetworkAdapter right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Whether two handles name different devices.
    /// </summary>
    /// <param name="left">First handle.</param>
    /// <param name="right">Second handle.</param>
    /// <returns>True when they name different devices.</returns>
    public static bool operator !=(NetworkAdapter left, NetworkAdapter right)
    {
        return !left.Equals(right);
    }
}
