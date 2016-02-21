using System;
using System.Linq;
using System.Threading.Tasks;

using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    public static class StringTest
    {
        static Debugger mDebugger = new Debugger("Tests", "String Tests");

        public static void Execute()
        {
            Assert.IsTrue(string.Empty == "", "string.Empty == \"\"");
            int xResult = string.Compare("a", "a");
            mDebugger.Send(xResult.ToString());
            Assert.IsTrue(xResult == 0, "string.Compare(\"a\", \"a\") == 0");

            Assert.IsTrue(
                string.Compare("abc", "abc") == 0, "string.Compare(\"abc\", \"abc\") == 0");
            Assert.IsTrue(("a" + "b") == "ab", "(\"a\" + \"b\") == \"ab\"");
            Assert.IsTrue(("a" + 'b') == "ab", "concatting 1 string and 1 character doesn\"t work");
            Assert.IsTrue(string.Concat("a", "b") == "ab", "string.Concat(\"a\", \"b\") == \"ab\"");
        }
    }
}
