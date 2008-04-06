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
            Console.WriteLine("Testing IPv4");

            //Create a Packet
            Packet ipv4Packet = new Packet();
            ipv4Packet.DestinationAddress = new Address(10, 0, 2, 2); //Virtual DHCP server in Qemu
            ipv4Packet.SourceAddress = new Address(10, 0, 2, 15); //Default IP address assigned in Qemu
            ipv4Packet.Protocol = Packet.Protocols.UDP;

            Console.WriteLine("Version: " + ipv4Packet.Version);
            Console.WriteLine("Data: " + ipv4Packet.Data);
            Console.WriteLine("Destination address: " + ipv4Packet.DestinationAddress.ToString());
            Console.WriteLine("Source address: " + ipv4Packet.SourceAddress.ToString());


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
            //nic.Transmit(ipv4Packet.RawBytes());

        }
    }
}
