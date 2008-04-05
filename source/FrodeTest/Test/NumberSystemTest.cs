using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware.Extension.NumberSystem;

namespace FrodeTest.Test
{
    class NumberSystemTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Testing Numbersystems");

            Console.WriteLine("0 as HEX: " + 0.ToHex());
            Console.WriteLine("10 as HEX: " + 10.ToHex());
            Console.WriteLine("15 as HEX: " + 15.ToHex());
            Console.WriteLine("16 as HEX: " + 16.ToHex());
            Console.WriteLine("17 as HEX: " + 17.ToHex());
            Console.WriteLine("1023 as HEX: " + 1023.ToHex());
            Console.WriteLine("99999 as HEX: " + 99999.ToHex());
        }
    }
}
