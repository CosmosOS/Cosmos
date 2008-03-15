using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Driver.RTL8139.Misc;

namespace FrodeTest.Test
{
    class BinaryHelperTest
    {
        public static void RunTest()
        {
            UInt32 high = 0x7FFFFFF;
            Console.WriteLine("Should be 255: " + BinaryHelper.GetByteFrom32bit(high, 0));

            UInt32 low = 0x0;
            Console.WriteLine("Should be 0: " + BinaryHelper.GetByteFrom32bit(low, 0));

            //10000000 00000000 00000000 00000000 = 0x80000000
            Console.WriteLine("Should be 128: " + BinaryHelper.GetByteFrom32bit(0x80000000, 24));

            //00000000 00000000 11111111 00000000
            UInt32 midlow = 0xFF00;
            Console.WriteLine("Should be 255: " + BinaryHelper.GetByteFrom32bit(midlow, 8));

            //00000000 11111111 00000000 00000000
            UInt32 midhigh = 0xFF0000;
            Console.WriteLine("Should be 255: " + BinaryHelper.GetByteFrom32bit(midhigh, 16));

            //000|11100 000|00000 00000000 00000000 = 0x1C000000
            Console.WriteLine("Should be 224: " + BinaryHelper.GetByteFrom32bit(0x1C000000, 21));

            Console.WriteLine("Should be 5: " + BinaryHelper.GetByteFrom32bit(80, 4));
        }
    }
}
