using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;

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

            UInt32 data = 0;
            Console.WriteLine("Should be FALSE: " + BinaryHelper.CheckBit(data, 0));

            data = UInt32.MaxValue;
            Console.WriteLine("Should be TRUE: " + BinaryHelper.CheckBit(data, 0));

            //Flip bit testing
            byte flipbits = 0xFF; // 1111 1111
            flipbits = BinaryHelper.FlipBit(flipbits, 0); //change to 1111 1110
            Console.WriteLine("Should be 254: " + flipbits);

            flipbits = 0x00;
            flipbits = BinaryHelper.FlipBit(flipbits, 1); //change to 0000 0010
            Console.WriteLine("Should be 2: " + flipbits);

            flipbits = 0xF0;
            flipbits = BinaryHelper.FlipBit(flipbits, 7); //change to 0111 0000
            Console.WriteLine("Should be 112: " + flipbits);

        }
    }
}
