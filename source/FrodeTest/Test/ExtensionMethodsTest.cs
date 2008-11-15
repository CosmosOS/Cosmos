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
            Console.WriteLine("-- Testing Extension methods --");

            const int number = 10;
            Check.Text = "Extension Method";
            Check.Validate(number.Tripple() == 30);
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
