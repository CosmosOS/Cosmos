using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel;

namespace FrodeTest.Test
{
    class NumberSystemTest
    {
        public static void RunTest()
        {
            Console.WriteLine("--Testing Numbersystems--");

            Console.WriteLine("0 as HEX: " + 0.ToHex());
            Console.WriteLine("10 as HEX: " + 10.ToHex());
            Console.WriteLine("15 as HEX: " + 15.ToHex());
            Console.WriteLine("16 as HEX: " + 16.ToHex());
            Console.WriteLine("17 as HEX: " + 17.ToHex());
            Console.WriteLine("1023 as HEX: " + 1023.ToHex());
            Console.WriteLine("99999 as HEX: " + 99999.ToHex());

            Console.WriteLine("0 as bin: " + 0.ToBinary());
            Console.WriteLine("1 as bin: " + 1.ToBinary());
            Console.WriteLine("2 as bin: " + 2.ToBinary());
            Console.WriteLine("3 as bin: " + 3.ToBinary());
            Console.WriteLine("4 as bin: " + 4.ToBinary());
            Console.WriteLine("128 as bin: " + 128.ToBinary());
            Console.WriteLine("255 as bin: " + 255.ToBinary());
            Console.WriteLine("257 as bin: " + 257.ToBinary());

        }
    }
}
