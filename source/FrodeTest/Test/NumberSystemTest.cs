using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrodeTest.Extension.NumberSystem;

namespace FrodeTest.Test
{
    class NumberSystemTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Testing Numbersystems");

            Console.WriteLine("0 as HEX: " + 0.AsHex());
            Console.WriteLine("10 as HEX: " + 10.AsHex());
            Console.WriteLine("15 as HEX: " + 15.AsHex());
            Console.WriteLine("16 as HEX: " + 16.AsHex());
            Console.WriteLine("17 as HEX: " + 17.AsHex());
            Console.WriteLine("1023 as HEX: " + 1023.AsHex());
            Console.WriteLine("99999 as HEX: " + 99999.AsHex());
        }
    }
}
