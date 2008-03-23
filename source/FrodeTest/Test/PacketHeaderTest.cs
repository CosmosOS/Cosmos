using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.Devices.RTL8139;

namespace FrodeTest.Test
{
    class PacketHeaderTest
    {
        public static void RunTest()
        {

            UInt16 data = 0xFFFF; //All 16 bits high
            var head = new PacketHeader(data);

            Console.WriteLine("Binary value in head: " + data);
            Console.WriteLine("IsRecieveOK - " + head.IsReceiveOk());
            Console.WriteLine("IsFrameAlignmentError - " + head.IsFrameAlignmentError());
            Console.WriteLine("IsCRCError - " + head.IsCRCError());
            Console.WriteLine("IsLongPacket - " + head.IsLongPacket());
            Console.WriteLine("IsRuntPacket - " + head.IsRuntPacket());
            Console.WriteLine("IsISEPacket - " + head.IsInvalidSymbolError());
            Console.WriteLine("IsBroadcast - " + head.IsBroadcastAddress());
            Console.WriteLine("IsPhysical - " + head.IsPhysicalAddressMatch());
            Console.WriteLine("IsMulticast - " + head.IsMulticastAddress());
        }
    }
}
