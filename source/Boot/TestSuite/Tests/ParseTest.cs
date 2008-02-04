using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
    public class ParseTest : TestBase
    {
        public override string Name
        {
            get { return "Parsing"; }
        }

        public override void Initialize()
        {
            
        }

        public override void Teardown()
        {
            
        }

        public override void Test()
        {
            Assert(short.Parse("5") == 5, "short.Parse(\"5\") == 5");
            Assert(short.Parse("500") == 500, "short.Parse(\"500\") == 500");
            Assert(short.Parse("-500") == -500, "short.Parse(\"-500\") == -500");
        }
    }
}
