using System;
using System.Collections.Generic;
using HW = Cosmos.Hardware;
using ARP = Cosmos.Sys.Network.TCPIP.ARP;
using ICMP = Cosmos.Sys.Network.TCPIP.ICMP;
using UDP = Cosmos.Sys.Network.TCPIP.UDP;

namespace Cosmos.Sys.Network
{
    /// <summary>
    /// Data Received function delegate for both UDP and TCP protocols
    /// </summary>
    /// <param name="source"><see cref="IPv4EndPoint"/> endpoint that data was received from</param>
    /// <param name="data">Data received in the packet</param>
    public delegate void DataReceived(IPv4EndPoint source, byte[] data);

    /// <summary>
    /// Implements a TCP/IP Stack on top of Cosmos
    /// </summary>
    public static class TCPIPStack
    {
        private static UInt16 sNextFragmentID;

        private static HW.TempDictionary<HW.Network.NetworkDevice> addressMap;
        private static List<IPv4Config> ipConfigs;
        private static HW.TempDictionary<DataReceived> udpClients;

        /// <summary>
        /// Initialize the TCP/IP Stack variables
        /// </summary>
        public static void Init()
        {
            addressMap = new HW.TempDictionary<HW.Network.NetworkDevice>();
            ipConfigs = new List<IPv4Config>();
            udpClients = new HW.TempDictionary<DataReceived>();
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

        /// <summary>
        /// Subscribe to a UDP port to listen to data received on a specific port number
        /// <remarks>Only one listener allowed</remarks>
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="callback"><see cref="DataReceived"/> delegate to call when data is received</param>
        public static void SubscribeUDPPort(UInt16 port, DataReceived callback)
        {
            if (udpClients.ContainsKey(port) == true)
            {
                throw new ArgumentException("Port is already subscribed to", "port");
            }

            udpClients.Add(port, callback);
        }

        /// <summary>
        /// Unsubscribe from existing subscription to a UDP port
        /// </summary>
        /// <param name="port">Port number to unsubscribe from</param>
        public static void UnsubscribeUDPPort(UInt16 port)
        {
            udpClients.Remove(port);
        }

        /// <summary>
        /// Send a UDP packet to a destination device
        /// </summary>
        /// <param name="dest">IP address of destination</param>
        /// <param name="srcPort">Source port</param>
        /// <param name="destPort">Destination port to send data to</param>
        /// <param name="data">Data to be sent</param>
        public static void SendUDP(IPv4Address dest, UInt16 srcPort, UInt16 destPort, byte[] data)
        {
            IPv4Address source = FindNetwork(dest);
            if (source == null)
            {
                Console.WriteLine("Destination Network Unreachable!!");
                return;
            }

            UDP.UDPPacket outgoing = new UDP.UDPPacket(source, dest, srcPort, destPort, data);
            TCPIP.IPv4OutgoingBuffer.AddPacket(outgoing);
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
                    case 17:
                        IPv4_UDPHandler(packetData);
                        break;
                }
            }
        }

        private static void IPv4_UDPHandler(byte[] packetData)
        {
            UDP.UDPPacket udp_packet = new UDP.UDPPacket(packetData);
            if (udpClients.ContainsKey(udp_packet.DestinationPort) == true)
            {
                DataReceived dlgt = udpClients[udp_packet.DestinationPort];
                if (dlgt != null)
                {
                    dlgt(new IPv4EndPoint(udp_packet.SourceIP, udp_packet.SourcePort), udp_packet.UDP_Data);
                }
            }
            /*byte[] abba = new byte[] { (byte)'a', (byte)'b', (byte)'b', (byte)'a' };

            UDP.UDPPacket reply = new UDP.UDPPacket(udp_packet.DestinationIP, udp_packet.SourceIP, 1234, 1534, abba);
            TCPIP.IPv4OutgoingBuffer.AddPacket(reply);*/
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
