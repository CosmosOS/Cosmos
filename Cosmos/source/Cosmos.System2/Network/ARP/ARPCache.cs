using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network.ARP
{
    internal static class ARPCache
    {
        private static TempDictionary<MACAddress> cache;

        private static void ensureCacheExists()
        {
            if (cache == null)
            {
                cache = new TempDictionary<MACAddress>();
            }
        }

        internal static bool Contains(IPv4.Address ipAddress)
        {
            ensureCacheExists();
            return cache.ContainsKey(ipAddress.Hash);
        }

        internal static void Update(IPv4.Address ipAddress, MACAddress macAddress)
        {
            ensureCacheExists();
            if (ipAddress == null)
            {
              global::System.Console.Write("");
            }
            UInt32 ip_hash = ipAddress.Hash;
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
