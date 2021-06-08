using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    static class StandardMethodTest
    {
        static class HexConverter
        {
            public static char ToCharUpper(int value)
            {
                value &= 0xF;
                value += 48;
                if (value > 57)
                {
                    value += 7;
                }
                return (char)value;
            }
        }

        public static void Execute()
        {
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(numbers[i], HexConverter.ToCharUpper(i), "ToCharUpper works: " + HexConverter.ToCharUpper(i));
            }
        }
    }
}
