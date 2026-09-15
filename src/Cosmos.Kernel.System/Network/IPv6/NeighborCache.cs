// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// Maps on-link IPv6 addresses to the MAC addresses Neighbor Discovery
/// resolved them to: the IPv6 counterpart of the ARP cache.
/// </summary>
internal static class NeighborCache
{
    private static readonly Dictionary<Address6, MACAddress> s_entries = [];

    /// <summary>
    /// Records or refreshes the link-layer address of a neighbor.
    /// </summary>
    /// <param name="address">The neighbor's IPv6 address; the unspecified address is ignored.</param>
    /// <param name="macAddress">Its MAC address.</param>
    internal static void Update(Address6 address, MACAddress macAddress)
    {
        if (address.IsZero)
        {
            return;
        }

        s_entries[address] = macAddress;
    }

    /// <summary>
    /// Resolves an IPv6 address from the cache.
    /// </summary>
    /// <param name="address">The neighbor's IPv6 address.</param>
    /// <returns>The MAC address, or null when the neighbor is unknown.</returns>
    internal static MACAddress? Resolve(Address6 address)
    {
        return s_entries.TryGetValue(address, out MACAddress? macAddress) ? macAddress : null;
    }
}
