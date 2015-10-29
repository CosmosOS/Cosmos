using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class StringTest
    {
        public static void Execute()
        {
            string x = "a";
            string y = "b";
            char z = 'b';
            string connected1 = x+y;
            string connected2 = x+Convert.ToString(z);
            Assert.IsTrue((connected1) == "ab", "concatting 2 string using + doesn't work");
            Assert.IsTrue((connected2) == "ab", "concatting 1 string and 1 character doesn't work");
        }
    }
}
