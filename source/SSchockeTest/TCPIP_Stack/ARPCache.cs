using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Playground.SSchocke.TCPIP_Stack
{
    public static class ARPCache
    {
        private static HW.TempDictionary<HW.Network.MACAddress> cache;

        static ARPCache()
        {
            cache = new HW.TempDictionary<Cosmos.Hardware.Network.MACAddress>();
        }

        public static bool Contains(IPv4Address ipAddress)
        {
            return cache.ContainsKey(ipAddress.To32BitNumber());
        }

        public static void Update(IPv4Address ipAddress, HW.Network.MACAddress macAddress)
        {
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
            if (cache.ContainsKey(ipAddress.To32BitNumber()) == false)
            {
                return null;
            }

            return cache[ipAddress.To32BitNumber()];
        }
    }
}
