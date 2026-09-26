// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv6;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// A handle to one of the network devices HAL enumeration registered. Obtained
/// from <see cref="NetworkManager.GetAdapter(int)"/> or
/// <see cref="NetworkManager.Primary"/>, and handed back to the ring to inspect
/// that device or to configure it with
/// <see cref="Config.IPConfig.Enable(NetworkAdapter, Address, Address, Address)"/>.
/// </summary>
/// <remarks>
/// The handle carries the generation stamp the manager gave the device when
/// it registered it, not the device's index. A network device a USB driver
/// published leaves the manager when its device is pulled out, the devices
/// registered after it move down one index, and a device registered later
/// can take the index it had: an index would then name another device,
/// while the stamp names the one registration it was taken from, or none.
/// A default-constructed value carries stamp 0, which no registration gets,
/// so it names no device rather than silently naming the first one.
/// </remarks>
public readonly struct NetworkAdapter : IEquatable<NetworkAdapter>
{
    /// <summary>
    /// The generation stamp the handle names its device by; 0 for the
    /// default-constructed handle, which belongs to no device.
    /// </summary>
    internal int Generation { get; }

    internal NetworkAdapter(int generation)
    {
        Generation = generation;
    }

    /// <summary>
    /// The device's index among the registered devices now, the one
    /// <see cref="NetworkManager.GetAdapter(int)"/> takes, or -1 for a
    /// handle that names none: a default handle, or one whose device was
    /// unregistered. A device registered after one that is unregistered
    /// moves down one index.
    /// </summary>
    public int Index => NetworkManager.IndexOf(Generation);

    /// <summary>
    /// Whether this handle still names a registered device. False for a
    /// default-constructed handle, and once the device was unregistered: a
    /// USB network device pulled out.
    /// </summary>
    public bool IsValid => Device is not null;

    internal INetworkDevice? Device => NetworkManager.FindDevice(Generation);

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
    /// <see cref="Config.IPConfig.Enable(NetworkAdapter, Address, Address, Address)"/>.
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
    /// The link-local IPv6 address the stack answers on for this device,
    /// <c>fe80::/64</c> with an interface identifier derived from the MAC
    /// address, or null while the device is unconfigured or the handle names
    /// none. It comes up with the device's IPv4 configuration.
    /// </summary>
    public Address6? LinkLocalAddress
    {
        get
        {
            INetworkDevice? device = Device;
            return device is null ? null : NetworkStack.LinkLocalOf(device);
        }
    }

    /// <summary>
    /// Whether two handles name the same registered device.
    /// </summary>
    /// <param name="other">The handle to compare against.</param>
    /// <returns>True when both were taken from the same registration of a device, or both are default handles.</returns>
    public bool Equals(NetworkAdapter other)
    {
        return Generation == other.Generation;
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
    /// Get a hash code derived from the device's generation stamp, which,
    /// unlike its index, never changes.
    /// </summary>
    /// <returns>Hash code for this handle.</returns>
    public override int GetHashCode()
    {
        return Generation;
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
