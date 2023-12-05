using Cosmos.HAL;
using Cosmos.System.Network;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.TestRunner;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sys = Cosmos.System;

namespace NetworkTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Tests");
        }

        protected override void Run()
        {
            try
            {
                /** 
                 * Packet creation and parsing tests
                **/

                /** Ethernet Packet Parsing Test **/
                byte[] ethernetPacketData = new byte[]
                {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x0C, 0x29, 0xD5, 0xDB, 0x9D, 0x08, 0x00
                };
                EthernetPacket ethernetPacket = new EthernetPacket(ethernetPacketData);
                Assert.AreEqual(ethernetPacketData, ethernetPacket.RawData, "Ethernet packet data is good.");

                /** IP Packet Parsing Test **/
                byte[] ipPacketData = new byte[]
                {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x0C, 0x29, 0xD5, 0xDB, 0x9D, 0x08, 0x00, 0x45, 0x00, 0x01, 0x16, 0x00, 0x00, 0x00, 0x00, 0x80, 0x11, 0x39,
                0xD8, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF
                };
                IPPacket ipPacket = new IPPacket(ipPacketData);
                Assert.AreEqual(ipPacketData, ipPacket.RawData, "IP packet data is good.");

                /** UDP Packet Parsing Test **/
                byte[] udpPacketData = new byte[]
                {
                0x98, 0xFA, 0x9B, 0xD4, 0xEB, 0x29, 0xD8, 0xCE, 0x3A, 0x89, 0x3E, 0xD9, 0x08, 0x00, 0x45, 0x00, 0x00, 0x22, 0x0C, 0x74, 0x40, 0x00, 0x40, 0x11, 0xAA,
                0xBE, 0xC0, 0xA8, 0x01, 0x02, 0xC0, 0xA8, 0x01, 0x46, 0x10, 0x92, 0x10, 0x92, 0x00, 0x0E, 0x37, 0x22, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x21
                };
                UDPPacket udpPacket = new UDPPacket(udpPacketData);
                Assert.AreEqual(udpPacketData, udpPacket.RawData, "UDP packet data is good.");

                /** DNS Packet Parsing Test **/
                byte[] dnsPacketData = new byte[]
                {
                0xB8, 0xD9, 0x4D, 0xC1, 0xA5, 0xFC, 0x98, 0xFA, 0x9B, 0xD4, 0xEB, 0x29, 0x08, 0x00, 0x45, 0x00, 0x00, 0x38, 0xC3, 0x1C, 0x00, 0x00, 0x80, 0x11, 0x00,
                0x00, 0xC0, 0xA8, 0x01, 0x46, 0xC0, 0xA8, 0x01, 0xFE, 0xF0, 0x66, 0x00, 0x35, 0x00, 0x24, 0x84, 0xCA, 0xD6, 0x80, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0x67, 0x69, 0x74, 0x74, 0x65, 0x72, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x01, 0x00, 0x01
                };
                DNSPacket dnsPacket = new DNSPacket(dnsPacketData);
                Assert.AreEqual(dnsPacketData, dnsPacket.RawData, "DNS packet data is good.");

                /** DHCP Packet Parsing Test **/
                byte[] dhcpPacketData = new byte[]
                {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xB8, 0xD9, 0x4D, 0xC1, 0xA5, 0xFC, 0x08, 0x00, 0x45, 0xC0, 0x01, 0x59, 0x46, 0x3F, 0x00, 0x00, 0x40, 0x11, 0x6F,
                0xEF, 0xC0, 0xA8, 0x01, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x43, 0x00, 0x44, 0x01, 0x45, 0xD3, 0xC8, 0x02, 0x01, 0x06, 0x00, 0x84, 0xA9, 0x5A, 0x66,
                0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0xA8, 0x01, 0x47, 0xC0, 0xA8, 0x01, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x34, 0xE1, 0x2D, 0xA3, 0x06,
                0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x63, 0x82, 0x53, 0x63, 0x35, 0x01, 0x05, 0x36, 0x04, 0xC0, 0xA8, 0x01, 0xFE, 0x33, 0x04, 0x00, 0x01, 0x51, 0x80, 0x3A, 0x04, 0x00,
                0x00, 0xA8, 0xC0, 0x3B, 0x04, 0x00, 0x01, 0x27, 0x50, 0x1C, 0x04, 0xC0, 0xA8, 0x01, 0xFF, 0x51, 0x12, 0x03, 0xFF, 0xFF, 0x44, 0x45, 0x53, 0x4B, 0x54,
                0x4F, 0x50, 0x2D, 0x49, 0x51, 0x48, 0x4A, 0x33, 0x31, 0x43, 0x06, 0x04, 0xC0, 0xA8, 0x01, 0xFE, 0x0F, 0x03, 0x6C, 0x61, 0x6E, 0x03, 0x04, 0xC0, 0xA8,
                0x01, 0xFE, 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0x00, 0xFF
                };
                DHCPPacket dhcpPacket = new DHCPPacket(dhcpPacketData);
                Assert.AreEqual(dhcpPacketData, dhcpPacket.RawData, "DHCP packet data is good.");

                /** TCP Packet Parsing Test **/
                byte[] tcpPacketData = new byte[]
                {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x08, 0x00, 0x45, 0x00, 0x00, 0x3C, 0x64, 0x92, 0x40, 0x00, 0x40, 0x06, 0x51,
                0xA2, 0xC0, 0xA8, 0x01, 0xD3, 0xC0, 0xA8, 0x01, 0x64, 0xA8, 0xAB, 0x10, 0x92, 0x67, 0x7C, 0xCE, 0x18, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x02, 0x72, 0x10,
                0x5F, 0xF0, 0x00, 0x00, 0x02, 0x04, 0x05, 0xB4, 0x04, 0x02, 0x08, 0x0A, 0x58, 0x1A, 0xAA, 0x8A, 0x00, 0x00, 0x00, 0x00, 0x01, 0x03, 0x03, 0x07
                };
                TCPPacket tcpPacket = new TCPPacket(tcpPacketData);
                Assert.AreEqual(tcpPacket.SourcePort, 43179, "TCP source port parsing OK.");
                Assert.AreEqual(tcpPacket.DestinationPort, 4242, "TCP destination port parsing OK.");
                Assert.AreEqual(tcpPacket.SequenceNumber, 0x677CCE18, "TCP sequence number parsing OK.");
                Assert.AreEqual(tcpPacket.AckNumber, 0, "TCP ACK number parsing OK.");
                Assert.AreEqual(tcpPacket.TCPFlags, (int)Flags.SYN, "TCP flag parsing OK.");
                Assert.AreEqual(tcpPacket.WindowSize, 29200, "TCP window size parsing OK.");
                Assert.AreEqual(tcpPacket.Checksum, 0x5FF0, "TCP checksum parsing OK.");
                Assert.AreEqual(tcpPacket.UrgentPointer, 0, "TCP urgent pointer parsing OK.");

                /** ARP Packet Parsing Test **/
                byte[] arpPacketData = new byte[]
                {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xB8, 0xD9, 0x4D, 0xC1, 0xA5, 0xFC, 0x08, 0x06, 0x00, 0x01, 0x08, 0x00, 0x06, 0x04, 0x00, 0x01, 0xB8, 0xD9, 0x4D,
                0xC1, 0xA5, 0xFC, 0xC0, 0xA8, 0x01, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0xA8, 0x01, 0x46, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
                ARPPacket arpPacket = new ARPPacket(arpPacketData);
                Assert.AreEqual(arpPacketData, arpPacket.RawData, "ARP packet data is good.");

                /** 
                 * Clients tests
                **/
                TestDhcpConnection();
                TestTcpConnection();
                TestDnsConnection();
                TestIcmpConnection();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }

        private void TestDhcpConnection()
        {
            Global.debugger.Send("Creating DHCP client...");

            using (var xClient = new DHCPClient())
            {
                xClient.SendDiscoverPacket();

                var ip = NetworkConfiguration.CurrentAddress.ToString();
                Global.debugger.Send("IP: " + ip);

                Assert.IsTrue(ip != null, "Received IP is valid.");
                Assert.IsFalse(NetworkConfiguration.CurrentAddress.Equals(Address.Zero), "Received IP is not ZERO, DHCP works");
            }
        }

        private void TestTcpConnection()
        {
            Global.debugger.Send("Creating TCP client...");

            using (var xClient = new TcpClient())
            {
                Global.debugger.Send("Creating IPAddress...");
                var address = new IPAddress(new byte[] { 127, 0, 0, 1 });
                Global.debugger.Send("Connecting to TCP server...");
                xClient.Connect(address, 12345);
                Global.debugger.Send("TcpClient connected.");
                NetworkStream stream = xClient.GetStream();
                Assert.IsTrue(xClient.Connected, "TCP connexion established.");

                byte[] receivedData = new byte[xClient.ReceiveBufferSize];
                int bytesRead = stream.Read(receivedData, 0, receivedData.Length);
                string receivedMessage = Encoding.ASCII.GetString(receivedData, 0, bytesRead);
                Assert.AreEqual(receivedMessage, "Hello from the testrunner!", "TCP receive works");

                Global.debugger.Send("TcpClient sending IP " + NetworkConfiguration.CurrentAddress.ToString());
                stream.Write(Encoding.ASCII.GetBytes(NetworkConfiguration.CurrentAddress.ToString()));
                Global.debugger.Send("TcpClient IP sent");

                // Envoyer un message au serveur
                string messageToSend = "cosmos is the best operating system uwu";
                byte[] dataToSend = Encoding.ASCII.GetBytes(messageToSend);
                Global.debugger.Send("Sending: " + messageToSend);
                stream.Write(dataToSend, 0, dataToSend.Length);

                byte[] receivedData2 = new byte[xClient.ReceiveBufferSize];
                int bytesRead2 = stream.Read(receivedData2, 0, receivedData2.Length);
                string receivedMessage2 = Encoding.ASCII.GetString(receivedData2, 0, bytesRead2);
                Assert.AreEqual(receivedMessage2, "COSMOS IS THE BEST OPERATING SYSTEM UWU", "TCP send works");

                string baseMessage = "This is a long TCP message for sequencing test...";
                string paddedMessage = baseMessage.PadRight(6000, '.');
                stream.Write(Encoding.ASCII.GetBytes(paddedMessage));
                Global.debugger.Send("Sent long data packet.");

                byte[] receivedData3 = new byte[xClient.ReceiveBufferSize];
                int bytesRead3 = stream.Read(receivedData3, 0, receivedData3.Length);
                Assert.AreEqual(bytesRead3, 6000, "TCP paquet sequencing works.");

                stream.Close();
            }

            Global.debugger.Send("Creating TCP server...");

            var xServer = new TcpListener(IPAddress.Any, 4343);
            xServer.Start();

            var client = xServer.AcceptTcpClient(); //blocking
            Assert.IsTrue(client.Connected, "Received new client! TCP connexion established.");
        }

        private void TestDnsConnection()
        {
            Global.debugger.Send("Creating DNS client...");

            using (var xClient = new DnsClient())
            {
                xClient.Connect(new Address(1, 1, 1, 1)); //Cloudflare DNS

                xClient.SendAsk("github.com");

                /** Receive DNS Response **/
                Address destination = xClient.Receive(); //can set a timeout value

                var ip = destination.ToString();
                Assert.IsTrue(ip != null, "Received IP is valid.");
                Assert.IsFalse(NetworkConfiguration.CurrentAddress.Equals(Address.Zero), "Received IP is not ZERO, DNS works");

                Global.debugger.Send("IP: " + ip);
            }
        }

        private void TestIcmpConnection()
        {
            Global.debugger.Send("Creating ICMP client...");

            using (var xClient = new ICMPClient())
            {
                xClient.Connect(new Address(127, 0, 0, 1)); //Cloudflare DNS

                xClient.SendEcho();

                var endpoint = new Sys.Network.IPv4.EndPoint(Address.Zero, 0);
                int time = xClient.Receive(ref endpoint);

                Assert.IsFalse(time == -1, "ICMP echo works");
            }
        }
    }
}
