using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;
using Cosmos.Playground.SSchocke.TCPIP_Stack;
using Cosmos.Kernel;

namespace Cosmos.Playground.SSchocke
{
    public static class TCPIP
    {
        public static IPv4Address OurAddress;
        private static UInt16 sNextFragmentID;

        public static UInt16 NextIPFragmentID()
        {
            return sNextFragmentID++;
        }

        public static void HandlePacket(HW.Network.NetworkDevice nic, byte[] packetData)
        {
            EthernetPacket packet = new EthernetPacket(packetData);
            /*Console.WriteLine("TCPIP: Ethernet Packet");
            Console.WriteLine("  From: " + packet.SourceMAC.ToString());
            Console.WriteLine("  To: " + packet.DestinationMAC.ToString());
            Console.WriteLine("  Type: " + packet.Type.ToHex(4));
            Console.WriteLine("-------------------------------");*/

            switch (packet.Type)
            {
                case 0x0806:
                    ARPHandler(nic, packetData);
                    break;
                case 0x0800:
                    IPv4Handler(nic, packetData);
                    break;
            }
        }

        private static void IPv4Handler(HW.Network.NetworkDevice nic, byte[] packetData)
        {
            IPPacket ip_packet = new IPPacket(packetData);
            /*Console.WriteLine("TCPIP: IP Packet");
            Console.WriteLine("  IP Version: " + ip_packet.IPVersion.ToString());
            Console.WriteLine("  Header Length: " + ip_packet.HeaderLength.ToString());
            Console.WriteLine("  ToS: " + ip_packet.TypeOfService.ToString());
            Console.WriteLine("  Packet Length: " + ip_packet.IPLength.ToString());
            Console.WriteLine("  Fragment ID: " + ip_packet.FragmentID.ToString());
            Console.WriteLine("  Fragment Offset: " + ip_packet.FragmentOffset.ToString());
            Console.WriteLine("  Flags: " + ip_packet.Flags.ToString());
            Console.WriteLine("  TTL: " + ip_packet.TTL.ToString());
            Console.WriteLine("  Protocol: " + ip_packet.Protocol.ToString());
            Console.WriteLine("  CRC: " + ip_packet.IPCRC.ToString());
            Console.WriteLine("  Source IP: " + ip_packet.SourceIP.ToString());
            Console.WriteLine("  Destination IP: " + ip_packet.DestinationIP.ToString());
            Console.WriteLine("-------------------------------");*/
            if (ip_packet.DestinationIP.CompareTo(OurAddress) == 0)
            {
                switch (ip_packet.Protocol)
                {
                    case 1:
                        IPv4_ICMPHandler(nic, packetData);
                        break;
                }
            }
        }

        private static void IPv4_ICMPHandler(HW.Network.NetworkDevice nic, byte[] packetData)
        {
            ICMPPacket icmp_packet = new ICMPPacket(packetData);
            /*Console.WriteLine("TCPIP: ICMP Packet");
            Console.WriteLine("  Type: " + icmp_packet.ICMP_Type.ToString());
            Console.WriteLine("  Code: " + icmp_packet.ICMP_Code.ToString());
            Console.WriteLine("  CRC: " + icmp_packet.ICMP_CRC.ToString());
            Console.WriteLine("-------------------------------");*/
            switch (icmp_packet.ICMP_Type)
            {
                case 8:
                    ICMPEchoRequest request = new ICMPEchoRequest(packetData);
                    /*Console.WriteLine("TCPIP: ICMP Echo Request Packet");
                    Console.WriteLine("  ID: " + request.ICMP_ID.ToString());
                    Console.WriteLine("  Sequence: " + request.ICMP_Sequence.ToString());
                    Console.WriteLine("  Data Length: " + request.ICMP_DataLength.ToString());
                    Console.WriteLine("-------------------------------");*/
                    Console.WriteLine("Sending ICMP Echo reply...");
                    ICMPEchoReply reply = new ICMPEchoReply(request);
                    /*Console.WriteLine("TCPIP: ICMP Echo Reply Packet");
                    Console.WriteLine("  CRC: " + reply.IPCRC.ToString());
                    Console.WriteLine("-------------------------------");*/

                    byte[] data = reply.GetBytes();
                    nic.QueueBytes(data, 0, data.Length);

                    /*Console.Write("Data: ");
                    WriteBinaryBuffer(data);*/

                    break;
            }
        }

        private static void ARPHandler(HW.Network.NetworkDevice nic, byte[] packetData)
        {
            ARPPacket arp_packet = new ARPPacket(packetData);
            /*Console.WriteLine("TCPIP: ARP Packet");
            Console.WriteLine("  Operation: " + arp_packet.Operation.ToHex(4));
            Console.WriteLine("  HardwareType: " + arp_packet.HardwareType.ToHex(4));
            Console.WriteLine("  ProtocolType: " + arp_packet.ProtocolType.ToHex(4));
            Console.WriteLine("-------------------------------");*/
            if (arp_packet.Operation == 0x01)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARPRequest_EthernetIPv4 arp_request = new ARPRequest_EthernetIPv4(packetData);
                    /*Console.WriteLine("TCPIP: ARP Request Packet");
                    Console.WriteLine("  Sender MAC: " + arp_request.SenderMAC.ToString());
                    Console.WriteLine("  Target MAC: " + arp_request.TargetMAC.ToString());
                    Console.WriteLine("  Sender IP: " + arp_request.SenderIP.ToString());
                    Console.WriteLine("  Target IP: " + arp_request.TargetIP.ToString());
                    Console.WriteLine("-------------------------------");*/

                    ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (OurAddress.CompareTo(arp_request.TargetIP) == 0)
                    {
                        Console.WriteLine("ARP Request Recvd: Reply with our MAC address...");

                        ARPReply_EthernetIPv4 reply =
                            new ARPReply_EthernetIPv4(nic.MACAddress, OurAddress, arp_request.SenderMAC, arp_request.SenderIP);

                        byte[] data = reply.GetBytes();
                        nic.QueueBytes(data, 0, data.Length);

                        /*Console.Write("Data: ");
                        WriteBinaryBuffer(data);*/
                    }
                }
            }
        }

        private static void WriteBinaryBuffer(byte[] buffer)
        {
            foreach (byte b in buffer)
            {
                Console.Write(b.ToHex(2) + " ");
            }
            Console.WriteLine();
        }
    }
}
