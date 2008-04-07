using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4;

namespace FrodeTest.Test
{
    class IPv4Test
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing IPv4 --");

            //Create a Packet
            Packet ipv4Packet = new Packet();
            ipv4Packet.DestinationAddress = new Address(10, 0, 2, 2); //Virtual DHCP server in Qemu
            ipv4Packet.SourceAddress = new Address(10, 0, 2, 15); //Default IP address assigned in Qemu
            
            ipv4Packet.Protocol = Packet.Protocols.UDP;
            ipv4Packet.FragmentFlags = Packet.Fragmentation.DoNotFragment;
            ipv4Packet.FragmentOffset = 0;
            ipv4Packet.HeaderChecksum = ipv4Packet.CalculateHeaderChecksum();
            ipv4Packet.TotalLength = ipv4Packet.CalculateTotalLength();
            
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
            /*foreach (byte b in ipv4Packet.RawBytes())
            {
                Console.Write(b + ":");
            }*/
            //foreach (UInt32 field in ipv4Packet.RawFields())
            //{
            //    Console.
           // }

            //Console.WriteLine("Total length: " + ipv4Packet.TotalLength);

            ////Send the Packet
            //var nics = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            //if (nics.Count == 0)
            //{
            //    Console.WriteLine("No networkcard RTL8139 found");
            //    return;
            //}

            //var nic = nics[0];
            //nic.Enable();
            //nic.InitializeDriver();
            ////nic.Transmit(ipv4Packet.RawBytes());

        }
    }
}
