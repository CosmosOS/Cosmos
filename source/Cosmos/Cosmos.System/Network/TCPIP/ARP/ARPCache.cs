using HW = Cosmos.Hardware;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    internal static class ARPCache
    {
        private static HW.TempDictionary<HW.Network.MACAddress> cache;

        private static void ensureCacheExists()
        {
            if (cache == null)
            {
                cache = new HW.TempDictionary<Cosmos.Hardware.Network.MACAddress>();
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
            if (cache.ContainsKey(ipAddress.To32BitNumber()) == false)
            {
                cache.Add(ipAddress.To32BitNumber(), macAddress);
            }
            else
            {
                cache[ipAddress.To32BitNumber()] = macAddress;
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
