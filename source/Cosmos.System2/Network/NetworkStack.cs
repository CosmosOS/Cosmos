using System;
using Sys = System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.IPv4;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Implement a Network Stack for all network devices and protocols
    /// </summary>
    public static class NetworkStack
    {

        public static Debugger debugger = new Debugger("System", "NetworkStack");

        internal static TempDictionary<NetworkDevice> AddressMap { get; private set; }

        /// <summary>
        /// Initialize the Network Stack to prepare it for operation
        /// </summary>
        public static void Init()
        {
            AddressMap = new TempDictionary<NetworkDevice>();

            // VMT Scanner issue workaround
            ARPPacket.VMTInclude();
            ARPPacket_Ethernet.VMTInclude();
            ARPReply_Ethernet.VMTInclude();
            ARPRequest_Ethernet.VMTInclude();
            ICMPPacket.VMTInclude();
            ICMPEchoReply.VMTInclude();
            ICMPEchoRequest.VMTInclude();
            UDPPacket.VMTInclude();
            //TCPPacket.VMTInclude();
        }

        /// <summary>
        /// Configure a IP configuration on the given network device.
        /// <remarks>Multiple IP Configurations can be made, like *nix environments</remarks>
        /// </summary>
        /// <param name="nic"><see cref="NetworkDevice"/> that will have the assigned configuration</param>
        /// <param name="config"><see cref="IPV4.Config"/> instance that defines the IP Address, Subnet
        /// Mask and Default Gateway for the device</param>
        public static void ConfigIP(NetworkDevice nic, Config config)
        {
            AddressMap.Add(config.IPAddress.Hash, nic);
            Config.Add(config);
            nic.DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            debugger.Send("Packet Received Length=" + packetData.Length.ToString());
            if (packetData == null)
            {
                debugger.Send("Error packet data null");
                return;
            }

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPPacket.ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPPacket.IPv4Handler(packetData);
                    break;
            }
        }

        /// <summary>
        /// Called continously to keep the Network Stack going.
        /// </summary>
        public static void Update()
        {
            OutgoingBuffer.Send();
        }
    }
}
