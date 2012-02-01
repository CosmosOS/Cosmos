using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SSchockeTest
{
    public static class NetworkStack
    {
        private static Dictionary<IPAddress, NetworkDevice> addressMap;
        private static List<IPAddress> ipConfigs;

        public static void Init()
        {
            ipConfigs = new List<IPAddress>();
        }

        public static void ConfigIP(NetworkDevice nic, IPAddress config)
        {
            ipConfigs.Add(config);
            nic.DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            Console.Write("Received Packet Length=");
            Console.WriteLine(packetData.Length);
            Console.WriteLine(BitConverter.ToString(packetData));

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    Console.WriteLine("Received ARP Packet");
                    //ARPHandler(packetData);
                    break;
                //case 0x0800:
                //    IPv4Handler(packetData);
                //    break;
            }
        }

        internal static void Update()
        {
            
        }
    }
}
