using System;
using Sys = System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.ARP;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Implement a Network Stack for all network devices and protocols
    /// </summary>
    public static class NetworkStack
    {
        internal static TempDictionary<NetworkDevice> AddressMap { get; private set; }

        /// <summary>
        /// Initialize the Network Stack to prepare it for operation
        /// </summary>
        public static void Init()
        {
            AddressMap = new TempDictionary<NetworkDevice>();

            // VMT Scanner issue workaround
            Cosmos.System.Network.ARP.ARPPacket.VMTInclude();
            //Cosmos.System.Network.IPv4.ARPPacket_Ethernet.VMTInclude();
            Cosmos.System.Network.IPv4.ARPReply_Ethernet.VMTInclude();
            Cosmos.System.Network.IPv4.ARPRequest_Ethernet.VMTInclude();
            Cosmos.System.Network.IPv4.ICMPPacket.VMTInclude();
            Cosmos.System.Network.IPv4.ICMPEchoReply.VMTInclude();
            Cosmos.System.Network.IPv4.ICMPEchoRequest.VMTInclude();
            Cosmos.System.Network.IPv4.UDPPacket.VMTInclude();
        }

        /// <summary>
        /// Configure a IP configuration on the given network device.
        /// <remarks>Multiple IP Configurations can be made, like *nix environments</remarks>
        /// </summary>
        /// <param name="nic"><see cref="Cosmos.HAL.NetworkDevice"/> that will have the assigned configuration</param>
        /// <param name="config"><see cref="Cosmos.System.Network.IPv4.Config"/> instance that defines the IP Address, Subnet
        /// Mask and Default Gateway for the device</param>
        public static void ConfigIP(NetworkDevice nic, IPv4.Config config)
        {
            AddressMap.Add(config.IPAddress.Hash, nic);
            IPv4.Config.Add(config);
            nic.DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            Sys.Console.Write("Received Packet Length=");
            if (packetData == null)
            {
                Sys.Console.WriteLine("**NULL**");
                return;
            }
            Sys.Console.WriteLine(packetData.Length);
            //Sys.Console.WriteLine(BitConverter.ToString(packetData));

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPPacket.ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPv4.IPPacket.IPv4Handler(packetData);
                    break;
            }
        }

        /// <summary>
        /// Called continously to keep the Network Stack going.
        /// </summary>
        public static void Update()
        {
            IPv4.OutgoingBuffer.Send();
        }
    }
}
