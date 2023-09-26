using System;
using System.Collections.Generic;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.ARP
{
    /// <summary>
    /// Manages the ARP (Address Resolution Protocol) cache.
    /// </summary>
    internal static class ARPCache
    {
        /// <summary>
        /// The cache map.
        /// </summary>
        public static Dictionary<uint, MACAddress> Cache;

        /// <summary>
        /// Ensures the cache map exists.
        /// </summary>
        private static void EnsureCacheExists()
        {
            Cache ??= new Dictionary<uint, MACAddress>();
        }

        /// <summary>
        /// Checks whether the ARP cache contains the given IP.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        internal static bool Contains(IPv4.Address ipAddress)
        {
            EnsureCacheExists();
            return Cache.ContainsKey(ipAddress.Hash);
        }

        /// <summary>
        /// Updates the ARP cache.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="macAddress">The MAC address.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        internal static void Update(IPv4.Address ipAddress, MACAddress macAddress)
        {
            EnsureCacheExists();
            uint ipHash = ipAddress.Hash;
            if (ipHash == 0)
            {
                return;
            }

            if (Cache.ContainsKey(ipHash) == false)
            {
                Cache.Add(ipHash, macAddress);
            }
            else
            {
                Cache[ipHash] = macAddress;
            }
        }

        /// <summary>
        /// Resolve an IP address to a MAC address using the ARP cache.
        /// </summary>
        /// <param name="ipAddress">IP address.</param>
        /// <returns>The resolved MAC address, or <see langword="null"/> if no cache entry for the given IP address exists.</returns>
        internal static MACAddress Resolve(IPv4.Address ipAddress)
        {
            EnsureCacheExists();
            if (Cache.ContainsKey(ipAddress.Hash) == false)
            {
                return null;
            }

            return Cache[ipAddress.Hash];
        }
    }
}
