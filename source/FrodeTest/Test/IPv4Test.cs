using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4;
using Cosmos.Hardware.Network.TCPIPModel.PhysicalLayer.Ethernet2;
using Cosmos.Hardware.Network.Devices.RTL8139;
using Cosmos.Hardware.Extension.NumberSystem;
using Cosmos.Hardware.Network;

namespace FrodeTest.Test
{
    class IPv4Test
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing IPv4 --");

            //Create a IPv4 Packet
            IPv4Packet ipv4Packet = new IPv4Packet();
            //ipv4Packet.DestinationAddress = new IPv4Address(10, 0, 2, 2); //Virtual DHCP server in Qemu
            //ipv4Packet.SourceAddress = new IPv4Address(10, 0, 2, 15); //Default IP address assigned in Qemu
            ipv4Packet.DestinationAddress = new IPv4Address(172,28,5,1);
            ipv4Packet.SourceAddress = new IPv4Address(172,28,6,6);
            ipv4Packet.TypeOfService = 0;
            ipv4Packet.Identification = 12345;
            ipv4Packet.FragmentFlags = IPv4Packet.Fragmentation.DoNotFragment;
            ipv4Packet.FragmentOffset = 0;
            ipv4Packet.Protocol = IPv4Packet.Protocols.TCP;
            List<byte> data = new List<byte>();
            data.Add(0xFF);
            data.Add(0xFE);
            data.Add(0xFD);
            data.Add(0xFC);
            data.Add(0xFB);
            data.Add(0xFA);
            ipv4Packet.Data = data;
            
            if (ipv4Packet.HeaderChecksum == 0)
                Console.WriteLine("IPv4 Packet Header Checksum turned off");
            else if(ipv4Packet.HeaderChecksum != ipv4Packet.CalculateHeaderChecksum())
                Console.WriteLine("IPv4 Packet Header Checksum failed!");

            ipv4Packet.HeaderLength = ipv4Packet.CalculateHeaderLength();
            ipv4Packet.TotalLength = ipv4Packet.CalculateTotalLength();
            ipv4Packet.HeaderChecksum = ipv4Packet.CalculateHeaderChecksum();

            Console.WriteLine("IPv4 Packet data:");
            foreach (byte b in ipv4Packet.RawBytes())
                Console.Write(b.ToHex() + ":");
            Console.WriteLine();
            
            Console.Write(ipv4Packet.ToString());

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

            Ethernet2Frame frame = new Ethernet2Frame();
            byte[] mDest = new byte[6];
            mDest[0] = 1;
            mDest[1] = 0;
            mDest[2] = 94;
            mDest[3] = 127;
            mDest[4] = 255;
            mDest[5] = 250;
            MACAddress macDest = new MACAddress(mDest);
            frame.Destination = macDest;

            byte[] mSrc = new byte[6];
            mSrc[0] = 0;
            mSrc[1] = 255;
            mSrc[2] = 99;
            mSrc[3] = 8;
            mSrc[4] = 252;
            mSrc[5] = 226;
            MACAddress macSrc = new MACAddress(mSrc);
            frame.Source = macSrc;
            frame.Payload = ipv4Packet.RawBytes();

            nic.TransmitBytes(frame.RawBytes());

        }
    }
}
