/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ARP Cache (Contains MAC/IP)
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using System.Collections.Generic;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.ARP
{
    /// <summary>
    /// ARPCache class.
    /// </summary>
    internal static class ARPCache
    {
        /// <summary>
        /// Cache.
        /// </summary>
        public static Dictionary<uint, MACAddress> cache;

        /// <summary>
        /// Ensure cache exists.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        private static void ensureCacheExists()
        {
            if (cache == null)
            {
                cache = new Dictionary<uint, MACAddress>();
            }
        }

        /// <summary>
        /// Check if ARP cache contains the given IP.
        /// </summary>
        /// <param name="ipAddress">IP address to check.</param>
        /// <returns>bool value.</returns>
        internal static bool Contains(IPv4.Address ipAddress)
        {
            ensureCacheExists();
            return cache.ContainsKey(ipAddress.Hash);
        }

        /// <summary>
        /// Update ARP cache.
        /// </summary>
        /// <param name="ipAddress">IP address.</param>
        /// <param name="macAddress">MAC address.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        internal static void Update(IPv4.Address ipAddress, MACAddress macAddress)
        {
            ensureCacheExists();
            uint ip_hash = ipAddress.Hash;
            if (ip_hash == 0)
            {
                return;
            }

            if (cache.ContainsKey(ip_hash) == false)
            {
                cache.Add(ip_hash, macAddress);
            }
            else
            {
                cache[ip_hash] = macAddress;
            }
        }

        /// <summary>
        /// Resolve ARP cache.
        /// </summary>
        /// <param name="ipAddress">IP address.</param>
        /// <returns>MAC address.</returns>
        internal static MACAddress Resolve(IPv4.Address ipAddress)
        {
            ensureCacheExists();
            if (cache.ContainsKey(ipAddress.Hash) == false)
            {
                return null;
            }

            return cache[ipAddress.Hash];
        }
    }
}
