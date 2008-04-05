using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;

namespace FrodeTest.Test
{
    class ExtensionMethodsTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Testing C# 3.0 Extension Methods");

            Int32 number = 10;
            Console.WriteLine("Trippling " + number + " gives " + number.Tripple());
        }
    }

    public static class IntExtender
    {
        public static Int32 Tripple(this int n)
        {
            return n * 3;
        }
    }
}
