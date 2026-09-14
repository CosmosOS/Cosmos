using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network.ARP;

/// <summary>
/// Manages the ARP (Address Resolution Protocol) cache.
/// </summary>
internal static class ArpCache
{
    /// <summary>
    /// The cache map.
    /// </summary>
    public static Dictionary<Address, MACAddress>? Cache;

    /// <summary>
    /// Ensures the cache map exists.
    /// </summary>
    [MemberNotNull(nameof(Cache))]
    private static void EnsureCacheExists()
    {
        Cache ??= [];
    }

    /// <summary>
    /// Updates the ARP cache.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="macAddress">The MAC address.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
    /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
    /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
    internal static void Update(Address ipAddress, MACAddress macAddress)
    {
        EnsureCacheExists();
        if (Equals(ipAddress, Address4.Zero))
        {
            return;
        }

        Cache[ipAddress] = macAddress;
    }

    /// <summary>
    /// Resolve an IP address to a MAC address using the ARP cache.
    /// </summary>
    /// <param name="ipAddress">IP address.</param>
    /// <returns>The resolved MAC address, or <see langword="null"/> if no cache entry for the given IP address exists.</returns>
    internal static MACAddress? Resolve(Address ipAddress)
    {
        EnsureCacheExists();

        return Cache.TryGetValue(ipAddress, out MACAddress? resolve) ? resolve : null;
    }
}
