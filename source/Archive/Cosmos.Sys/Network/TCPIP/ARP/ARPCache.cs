using System;
using HW = Cosmos.Hardware2;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    internal static class ARPCache
    {
        private static HW.TempDictionary<HW.Network.MACAddress> cache;

        private static void ensureCacheExists()
        {
            if (cache == null)
            {
                cache = new HW.TempDictionary<Cosmos.Hardware2.Network.MACAddress>();
            }
        }

        public static bool Contains(IPv4Address ipAddress)
        {
            ensureCacheExists();
            return cache.ContainsKey(ipAddress.To32BitNumber());
        }

        public static void Update(IPv4Address ipAddress, HW.Network.MACAddress macAddress)
        {
            ensureCacheExists();
            UInt32 ip_hash = ipAddress.To32BitNumber();
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

        public static HW.Network.MACAddress Resolve(IPv4Address ipAddress)
        {
            ensureCacheExists();
            if (cache.ContainsKey(ipAddress.To32BitNumber()) == false)
            {
                return null;
            }

            return cache[ipAddress.To32BitNumber()];
        }
    }
}
