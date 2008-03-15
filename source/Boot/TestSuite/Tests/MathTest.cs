using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
    public class MathTest : TestBase
    {
        public override string Name
        {
            get { return "Math"; }
        }

        public override void Initialize()
        {

        }

        public override void Teardown()
        {
        }

        public override void Test()
        {
            Assert(1 + 1 == 2, "1 + 1 == 2");
            Assert(-1 + 2 == 1, "-1 + 2 == 1");
            Assert(2 * 2 == 4, "2 * 2 == 4");
            Assert(6 / 2 == 3, "6 / 2 == 3");
            Assert(5 - 2 == 3, "5 - 2 == 3");
            Assert(2 + 5 * 2 == 12, "2 + 5 * 2 == 12");
            Assert((2 + 5) * 2 == 14, "(2 + 5) * 2 == 14");
			long al = 0x1FFFFFFFF;
			long bl = 0x20;//1L;
			Assert(al != bl, "Int64 Inequality");
			Assert(0x1FFFFFFFF + 0x01L == 0x200000000, "0x1FFFFFFFF + 0x01L == 0x200000000");

            UInt32 a = 5;
            UInt32 b = 5;
            Assert(a == b, "UInt32 Equality");
            b = 10;
            Assert(a != b, "UInt32 Inequality");
        }
    }
}
