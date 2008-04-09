using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4;
using Cosmos.Hardware.Network.Devices.RTL8139;

namespace FrodeTest.Test
{
    class IPv4Test
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing IPv4 --");

            //Create a Packet
            IPv4Packet ipv4Packet = new IPv4Packet();
            ipv4Packet.DestinationAddress = new IPv4Address(10, 0, 2, 2); //Virtual DHCP server in Qemu
            ipv4Packet.SourceAddress = new IPv4Address(10, 0, 2, 15); //Default IP address assigned in Qemu
            ipv4Packet.Protocol = IPv4Packet.Protocols.UDP;
            ipv4Packet.FragmentFlags = IPv4Packet.Fragmentation.DoNotFragment;
            ipv4Packet.FragmentOffset = 0;
            ipv4Packet.HeaderChecksum = ipv4Packet.CalculateHeaderChecksum();
            ipv4Packet.TotalLength = ipv4Packet.CalculateTotalLength();

            List<byte> data = new List<byte>();
            data.Add(0xFF);
            data.Add(0xFE);
            data.Add(0xFD);
            data.Add(0xFC);
            ipv4Packet.Data = data;
            
            if (ipv4Packet.HeaderChecksum == 0)
                Console.WriteLine("IPv4 Packet Header Checksum turned off");
            else if(ipv4Packet.HeaderChecksum != ipv4Packet.CalculateHeaderChecksum())
                Console.WriteLine("IPv4 Packet Header Checksum failed!");



            Console.WriteLine("IPv4 Packet data:");
            Console.WriteLine("Version: " + ipv4Packet.Version);
            Console.WriteLine("Time to Live: " + ipv4Packet.TimeToLive);
            //Console.WriteLine("Data: " + ipv4Packet.Data);
            //Console.WriteLine("Protocol: " + Enum.GetName(typeof(Packet.Protocols), ipv4Packet.Protocol).ToString());
            Console.WriteLine("Source address: " + ipv4Packet.SourceAddress.ToString());
            Console.WriteLine("Destination address: " + ipv4Packet.DestinationAddress.ToString());
            
            Console.WriteLine("Raw bytes:");
            foreach (byte b in ipv4Packet.RawBytes())
            {
                Console.Write(b + ":");
            }
            Console.WriteLine();
            Console.WriteLine("Total length: " + ipv4Packet.TotalLength);


            //Send the Packet
            var nics = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            if (nics.Count == 0)
            {
                Console.WriteLine("No networkcard RTL8139 found");
                return;
            }

            var nic = nics[0];
            nic.Enable();
            nic.InitializeDriver();

            Packet physicalPacket = new Packet(new PacketHeader(0xFF), ipv4Packet.RawBytes());
            nic.Transmit(physicalPacket);

        }
    }
}
