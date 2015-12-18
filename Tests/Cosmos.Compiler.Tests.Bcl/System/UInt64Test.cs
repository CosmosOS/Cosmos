using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    using Cosmos.TestRunner;

    public static class UInt64Test
    {
        public static void Execute()
        {
            var xTest = TestMethod(0);
            Assert.IsTrue(xTest.Length == 0, "UInt64 test failed.");
        }

        public static ulong[] TestMethod(ulong aParam1, uint aParam2 = 0)
        {
            var xReturn = new ulong[0];
            ulong xParam1 = aParam1;
            ulong xValue;

            TestMethod2(xParam1, out xValue);
            Array.Resize(ref xReturn, xReturn.Length + 1);
            xReturn[xReturn.Length - 1] = xValue;

            return xReturn;
        }

        public static void TestMethod2(ulong aParam1, out ulong aValue)
        {
            aValue = 8;
            switch (aParam1)
            {
                case 1:
                    aValue = 8 & 0x0FFF;
                    break;
                case 2:
                    aValue = 8;
                    break;
                case 3:
                    aValue = 8 & 0x0FFFFFFF;
                    break;
            }
        }

    }
}
