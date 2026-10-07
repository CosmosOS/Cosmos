// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Network;

namespace Cosmos.Kernel.System.Network.Protocols.Arp;

/// <summary>
/// Manages the ARP (Address Resolution Protocol) cache.
/// </summary>
internal static class ArpCache
{
    /// <summary>
    /// The MAC address last seen for each IP address; <see langword="null"/> until the first update or lookup.
    /// </summary>
    public static Dictionary<Address, MacAddress>? Cache;

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
    internal static void Update(Address ipAddress, MacAddress macAddress)
    {
        EnsureCacheExists();

        // 0.0.0.0 is the sender of an ARP probe or of a DHCP client with no lease yet: no address to cache.
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
    internal static MacAddress? Resolve(Address ipAddress)
    {
        EnsureCacheExists();

        return Cache.TryGetValue(ipAddress, out MacAddress? resolve) ? resolve : null;
    }
}
