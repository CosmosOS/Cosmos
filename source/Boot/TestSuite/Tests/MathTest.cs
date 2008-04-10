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
			Assert(-1 < 1, "-1 < 1");
			Assert(0xFFFFFFFFu > 1u, "0xFFFFFFFFu > 1u");
            Assert(2 + 5 * 2 == 12, "2 + 5 * 2 == 12");
            Assert((2 + 5) * 2 == 14, "(2 + 5) * 2 == 14");
			long al = 0x1FFFFFFFF;
			long bl = 0x300000000;	//1L;
			al += 0x01;				//al == 0x200000000
			bl -= 0xFFFFFFFFL;		//bl == 0x200000001
			al -= 0x02;				//al == 0x1FFFFFFFE
			bl -= 0x03;				//bl == 0x1FFFFFFFE
			Assert(al == bl, "Int64 operations");
			Assert((-41L) - (-31L) == -10L, "Int64 negatives");

            UInt32 a = 5;
            UInt32 b = 5;
            Assert(a == b, "UInt32 Equality");
            b = 10;
            Assert(a != b, "UInt32 Inequality");
        }
    }
}
