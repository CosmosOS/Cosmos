using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
    public class MathTest : TestBase
    {
        public override string Name
        {
            get { return "Add"; }
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
        }
    }
}
