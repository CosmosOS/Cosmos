using System;
using System.Collections.Generic;
using HW = Cosmos.Hardware2;
using ARP = Cosmos.Sys.Network.TCPIP.ARP;
using ICMP = Cosmos.Sys.Network.TCPIP.ICMP;
using UDP = Cosmos.Sys.Network.TCPIP.UDP;
using TCP = Cosmos.Sys.Network.TCPIP.TCP;

namespace Cosmos.Sys.Network
{
    /// <summary>
    /// Data Received function delegate for UDP protocol
    /// </summary>
    /// <param name="source"><see cref="IPv4EndPoint"/>Endpoint that data was received from</param>
    /// <param name="data">Data received in the packet</param>
    public delegate void DataReceived(IPv4EndPoint source, byte[] data);

    /// <summary>
    /// New TCP Client connection delegate
    /// </summary>
    /// <param name="client">Instance of the <see cref="TcpClient"/> that represents the connection</param>
    public delegate void ClientConnected(TcpClient client);
    /// <summary>
    /// TCP Client Data Received delegate
    /// </summary>
    /// <param name="client"><see cref="TcpClient"/> instance that data was received on</param>
    /// <param name="data">Byte buffer of the received data</param>
    public delegate void ClientDataReceived(TcpClient client, byte[] data);

    /// <summary>
    /// TCP Client disconnection delegate
    /// </summary>
    /// <param name="client">Instance of the <see cref="TcpClient"/> that represents the connection</param>
    public delegate void ClientDisconnected(TcpClient client);

    /// <summary>
    /// Implements a TCP/IP Stack on top of Cosmos
    /// </summary>
    public static class TCPIPStack
    {
        private static UInt16 sNextFragmentID;

        private static HW.TempDictionary<HW.Network.NetworkDevice> addressMap;
        private static List<IPv4Config> ipConfigs;
        private static HW.TempDictionary<DataReceived> udpClients;
        private static HW.TempDictionary<ClientConnected> tcpListeners;
        internal static List<TCP.TCPConnection> tcpSockets;

        /// <summary>
        /// Initialize the TCP/IP Stack variables
        /// </summary>
        public static void Init()
        {
            addressMap = new HW.TempDictionary<HW.Network.NetworkDevice>();
            ipConfigs = new List<IPv4Config>();
            udpClients = new HW.TempDictionary<DataReceived>();
            tcpListeners = new HW.TempDictionary<ClientConnected>();
            tcpSockets = new List<TCP.TCPConnection>();
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

        /// <summary>
        /// Add a TCP listener to the specified port
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="connectCallback">Callback function that is called when a new client connects</param>
        public static void AddTcpListener(UInt16 port, ClientConnected connectCallback)
        {
            if (tcpListeners.ContainsKey(port) == true)
            {
                throw new ArgumentException("Port is already subscribed to", "port");
            }

            tcpListeners.Add(port, connectCallback);
        }

        internal static UInt16 NextIPFragmentID()
        {
            return sNextFragmentID++;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
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

            if ((addressMap.ContainsKey(ip_packet.DestinationIP.To32BitNumber()) == true) ||
                (ip_packet.DestinationIP.address[3] == 255))
            {
                switch (ip_packet.Protocol)
                {
                    case 1:
                        IPv4_ICMPHandler(packetData);
                        break;
                    case 6:
                        IPv4_TCPHandler(packetData);
                        break;
                    case 17:
                        IPv4_UDPHandler(packetData);
                        break;
                }
            }
        }

        private static void IPv4_TCPHandler(byte[] packetData)
        {
            TCP.TCPPacket tcp_packet = new TCP.TCPPacket(packetData);
            if (tcp_packet.Syn == true)
            {
                if (tcpListeners.ContainsKey(tcp_packet.DestinationPort) == true)
                {
                    TCP.TCPConnection connection = new TCP.TCPConnection(tcp_packet.SourceIP, tcp_packet.SourcePort, tcp_packet.DestinationIP, 
                        tcp_packet.DestinationPort, tcp_packet.SequenceNumber, TCP.TCPConnection.State.SYN_RECVD);

                    tcpSockets.Add(connection);

                    TCP.TCPPacket syn_ack = new TCP.TCPPacket(connection, connection.LocalSequenceNumber, 
                                                                ++connection.RemoteSequenceNumber, 0x12, 8192, 2);
                    syn_ack.AddMSSOption(1360);
                    syn_ack.AddSACKOption();

                    TCPIP.IPv4OutgoingBuffer.AddPacket(syn_ack);

                    return;
                }
            }

            TCP.TCPConnection active_connection = null;
            for (int c = 0; c < tcpSockets.Count; c++)
            {
                if ((tcpSockets[c].RemoteIP.CompareTo(tcp_packet.SourceIP) == 0) &&
                    (tcpSockets[c].RemotePort == tcp_packet.SourcePort) &&
                    (tcpSockets[c].LocalPort == tcp_packet.DestinationPort))
                {
                    active_connection = tcpSockets[c];
                    break;
                }
            }

            if (active_connection == null)
            {
                TCP.TCPPacket reset_packet = new TCP.TCPPacket(tcp_packet.DestinationIP, tcp_packet.SourceIP, tcp_packet.DestinationPort,
                                                                tcp_packet.SourcePort, 0, (tcp_packet.SequenceNumber + 1), 0x14, 8192);
                TCPIP.IPv4OutgoingBuffer.AddPacket(reset_packet);
                return;
            }

            if (active_connection.ConnectionState == TCP.TCPConnection.State.SYN_RECVD)
            {
                if ((tcp_packet.Ack == true) && ((active_connection.LocalSequenceNumber + 1) == tcp_packet.AckNumber))
                {
                    active_connection.LocalSequenceNumber++;
                    active_connection.ConnectionState = TCP.TCPConnection.State.ESTABLISHED;

                    ClientConnected connectCallback = tcpListeners[tcp_packet.DestinationPort];

                    connectCallback(new TcpClient(active_connection));
                }
            }
            else if (active_connection.ConnectionState == TCP.TCPConnection.State.SYN_SENT)
            {
                if ((tcp_packet.Syn == true) && (tcp_packet.Ack == true) && ((active_connection.LocalSequenceNumber + 1) == tcp_packet.AckNumber))
                {
                    active_connection.LocalSequenceNumber++;
                    active_connection.RemoteSequenceNumber = tcp_packet.SequenceNumber + 1;
                    active_connection.ConnectionState = TCP.TCPConnection.State.ESTABLISHED;

                    TCP.TCPPacket ack = new TCP.TCPPacket(active_connection, active_connection.LocalSequenceNumber,
                                                        active_connection.RemoteSequenceNumber, 0x10, 8192);
                    TCPIP.IPv4OutgoingBuffer.AddPacket(ack);
                }
            }
            else if (active_connection.ConnectionState == TCP.TCPConnection.State.ESTABLISHED)
            {
                if (tcp_packet.Ack == true)
                {
                    //active_connection.LocalSequenceNumber = tcp_packet.AckNumber;
                }

                if (tcp_packet.TCP_DataLength > 0)
                {
                    active_connection.RemoteSequenceNumber += tcp_packet.TCP_DataLength;

                    TCP.TCPPacket ack = new TCP.TCPPacket(active_connection, active_connection.LocalSequenceNumber,
                                                        active_connection.RemoteSequenceNumber, 0x10, 8192);
                    TCPIP.IPv4OutgoingBuffer.AddPacket(ack);

                    active_connection.client.dataReceived(tcp_packet.TCP_Data);
                }

                if (tcp_packet.Fin == true)
                {
                    active_connection.client.disconnect();
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
                        //Console.WriteLine("ARP Request Recvd from " + arp_request.SenderIP.ToString());
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

                    //Console.WriteLine("ARP Reply Recvd for IP=" + arp_reply.SenderIP.ToString());
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

        internal static bool IsLocalAddress(IPv4Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if ((ipConfigs[c].IPAddress.To32BitNumber() & ipConfigs[c].SubnetMask.To32BitNumber()) ==
                    (destIP.To32BitNumber() & ipConfigs[c].SubnetMask.To32BitNumber()))
                {
                    return true;
                }
            }

            return false;
        }

        internal static IPv4Address FindRoute(IPv4Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if (ipConfigs[c].DefaultGateway.CompareTo(IPv4Address.Zero) != 0)
                {
                    return ipConfigs[c].DefaultGateway;
                }
            }

            return null;
        }
    }
}
