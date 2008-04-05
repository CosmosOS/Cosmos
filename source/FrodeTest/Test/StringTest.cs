using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class StringTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Testing String");

            Console.Write("LeftPadding: ");
            string hex = "F";
            hex = hex.PadLeft(2, '0');
            Console.WriteLine(hex);
        }
    }
}
