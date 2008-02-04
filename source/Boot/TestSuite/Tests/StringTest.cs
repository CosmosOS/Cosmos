using System;
using System.Collections.Generic;
using System.Text;

namespace TestSuite.Tests
{
    public class StringTest : TestBase
    {
        public override string Name
        {
            get { return "Strings"; }
        }

        public override void Initialize()
        {
            
        }

        public override void Teardown()
        {
            
        }

        public override void Test()
        {
            Assert("ABCDEFG".IndexOf('E') == 4, "\"ABCDEFG\".IndexOf('E') == 4");
            Assert("0123456789".IndexOf('5') == 5, "\"0123456789\".IndexOf('5') == 5");
            Assert("0123456789"[5] == '5', "\"0123456789\"[5] == '5'");
            Assert("a" != "b", "\"a\" != \"b\"");
            Assert('a'.ToString() == "a", "'a'.ToString() == \"a\"");
            Assert("abc" == "abc", "\"abc\" == \"abc\"");

            Assert(("a" + "b") == "ab", "\"a\" + \"b\" == \"a\" + \"b\"");

			//StringBuilder builder = new StringBuilder("a");
			//builder.Append("b");
			//Assert(builder.ToString() == "ab", "(stringbuilder)");
        }
    }
}
