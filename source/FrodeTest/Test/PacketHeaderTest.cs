using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    class PacketHeaderTest
    {
        public static void RunTest()
        {

            UInt16 data = 0;
            Cosmos.Driver.RTL8139.PacketHeader head = new Cosmos.Driver.RTL8139.PacketHeader(data);

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
