using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Tests
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
        }
    }
}
