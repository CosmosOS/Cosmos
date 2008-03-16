using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
	public class OperatorsTest: TestBase
    {
        public override string Name
        {
            get { return "Operators"; }
        }

        public override void Initialize()
        {
            
        }

        public override void Teardown()
        {
            
        }

        public override void Test()
        {
            //switch block
			int c = 42;
			switch (c)
			{
			case 192:
				Assert(192 == 42,Name);
				break;
			case 224:
				Assert(224 == 42,Name);
				break;
			case 225:
				Assert(225 == 42,Name);
				break;
			case 232:
				Assert(232 == 42,Name);
				break;
			case 235:
				Assert(235 == 42,Name);
				break;
			default:
				Assert(c == 42, Name);
				break;
			}
        }
	}
}
