using System;
using System.Collections.Generic;
using HW = Cosmos.Hardware;
using ARP = Cosmos.Sys.Network.TCPIP.ARP;
using ICMP = Cosmos.Sys.Network.TCPIP.ICMP;

namespace Cosmos.Sys.Network
{
    /// <summary>
    /// Implements a TCP/IP Stack on top of Cosmos
    /// </summary>
    public static class TCPIPStack
    {
        private static UInt16 sNextFragmentID;

        private static HW.TempDictionary<HW.Network.NetworkDevice> addressMap;
        private static List<IPv4Config> ipConfigs;

        /// <summary>
        /// Initialize the TCP/IP Stack variables
        /// </summary>
        public static void Init()
        {
            addressMap = new HW.TempDictionary<HW.Network.NetworkDevice>();
            ipConfigs = new List<IPv4Config>();
        }

        /// <summary>
        /// Configure the IP address setup for a given network card
        /// </summary>
        /// <param name="nic">Network Device to configure</param>
        /// <param name="config">Configuration</param>
        public static void ConfigIP(HW.Network.NetworkDevice nic, IPv4Config config)
        {
            addressMap.Add(config.IPAddress.To32BitNumber(), nic);
            ipConfigs.Add(config);
            nic.DataReceived += HandlePacket;
        }

        /// <summary>
        /// This function must be called repeatedly to keep the TCP/IP Stack going.
        /// <remarks>Will later be called by a background thread in the kernel</remarks>
        /// </summary>
        public static void Update()
        {
            TCPIP.IPv4OutgoingBuffer.Send();
        }

        /// <summary>
        /// Can be used to test pinging to a network address from Cosmos
        /// </summary>
        /// <param name="dest">IP Address of destination</param>
        public static void Ping(IPv4Address dest)
        {
            IPv4Address source = FindNetwork(dest);
            if (source == null)
            {
                Console.WriteLine("Destination Network Unreachable!!");
                return;
            }

            ICMP.ICMPEchoRequest request = new ICMP.ICMPEchoRequest(source, dest, 0x10, 1);
            TCPIP.IPv4OutgoingBuffer.AddPacket(request);
        }

        internal static UInt16 NextIPFragmentID()
        {
            return sNextFragmentID++;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            TCPIP.EthernetPacket packet = new TCPIP.EthernetPacket(packetData);

            switch (packet.Type)
            {
                case 0x0806:
                    ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPv4Handler(packetData);
                    break;
            }
        }

        private static void IPv4Handler(byte[] packetData)
        {
            TCPIP.IPPacket ip_packet = new TCPIP.IPPacket(packetData);

            ARP.ARPCache.Update(ip_packet.SourceIP, ip_packet.SourceMAC);

            if( addressMap.ContainsKey(ip_packet.DestinationIP.To32BitNumber()) == true )
            {
                switch (ip_packet.Protocol)
                {
                    case 1:
                        IPv4_ICMPHandler(packetData);
                        break;
                }
            }
        }

        private static void IPv4_ICMPHandler(byte[] packetData)
        {
            ICMP.ICMPPacket icmp_packet = new ICMP.ICMPPacket(packetData);
            switch (icmp_packet.ICMP_Type)
            {
                case 0:
                    ICMP.ICMPEchoReply recvd_reply = new ICMP.ICMPEchoReply(packetData);
                    Console.WriteLine("Received ICMP Echo reply from " + recvd_reply.SourceIP.ToString());
                    break;
                case 8:
                    ICMP.ICMPEchoRequest request = new ICMP.ICMPEchoRequest(packetData);
                    ICMP.ICMPEchoReply reply = new ICMP.ICMPEchoReply(request);
                    Console.WriteLine("Sending ICMP Echo reply to " + reply.DestinationIP.ToString());
                    TCPIP.IPv4OutgoingBuffer.AddPacket(reply);
                    break;
            }
        }

        private static void ARPHandler(byte[] packetData)
        {
            ARP.ARPPacket arp_packet = new ARP.ARPPacket(packetData);
            if (arp_packet.Operation == 0x01)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARP.ARPRequest_EthernetIPv4 arp_request = new ARP.ARPRequest_EthernetIPv4(packetData);

                    ARP.ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (addressMap.ContainsKey(arp_request.TargetIP.To32BitNumber()) == true)
                    {
                        Console.WriteLine("ARP Request Recvd from " + arp_request.SenderIP.ToString());
                        HW.Network.NetworkDevice nic = addressMap[arp_request.TargetIP.To32BitNumber()];

                        ARP.ARPReply_EthernetIPv4 reply =
                            new ARP.ARPReply_EthernetIPv4(nic.MACAddress, arp_request.TargetIP, arp_request.SenderMAC, arp_request.SenderIP);

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arp_packet.Operation == 0x02)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARP.ARPReply_EthernetIPv4 arp_reply = new ARP.ARPReply_EthernetIPv4(packetData);
                    ARP.ARPCache.Update(arp_reply.SenderIP, arp_reply.SenderMAC);

                    Console.WriteLine("ARP Reply Recvd for IP=" + arp_reply.SenderIP.ToString());
                    TCPIP.IPv4OutgoingBuffer.ARPCache_Update(arp_reply);
                }
            }
        }

        internal static HW.Network.NetworkDevice FindInterface(IPv4Address sourceIP)
        {
            return addressMap[sourceIP.To32BitNumber()];
        }

        internal static IPv4Address FindNetwork(IPv4Address destIP)
        {
            IPv4Address default_gw = null;

            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if ((ipConfigs[c].IPAddress.To32BitNumber() & ipConfigs[c].SubnetMask.To32BitNumber()) ==
                    (destIP.To32BitNumber() & ipConfigs[c].SubnetMask.To32BitNumber()))
                {
                    return ipConfigs[c].IPAddress;
                }
                if ((default_gw == null) && (ipConfigs[c].DefaultGateway.CompareTo(IPv4Address.Zero) != 0))
                {
                    default_gw = ipConfigs[c].IPAddress;
                }
            }

            return default_gw;
        }
    }
}
