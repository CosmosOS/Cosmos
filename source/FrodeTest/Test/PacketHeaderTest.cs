using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    class PacketHeaderTest
    {
        public static void RunTest()
        {

            UInt16 data = 4;
            Cosmos.Driver.RTL8139.PacketHeader head = new Cosmos.Driver.RTL8139.PacketHeader(data);
            Console.WriteLine("IsRecieveOK - " + head.IsReceiveOk().ToString());
            if (!head.IsReceiveOk())
                Console.WriteLine("IsRecieveOK - returns false!");

            Console.WriteLine("IsFrameAlignmentError - " + head.IsFrameAlignmentError().ToString());
            if (!head.IsFrameAlignmentError())
                Console.WriteLine("IsFrameAlignmentError - returns false!");
        }
    }
}
