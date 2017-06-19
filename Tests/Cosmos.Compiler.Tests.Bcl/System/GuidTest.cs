using Sys = System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class GuidTest
    {

        public static void Execute()
        {

            //dont know how else to test this

           Sys.Guid test = Sys.Guid.NewGuid();

            string teststring = test.ToString();

            Assert.IsTrue((teststring.Length == 36), "invalid Length");

            Assert.IsTrue((teststring.Substring(14, 1) == "4"), "7th byte invalid (not 4)");

            bool test9thbyte = false;

            if (teststring.Substring(19, 1) == "8")
            {
                test9thbyte = true;
            }
            else if (teststring.Substring(19, 1) == "9")
            {
                test9thbyte = true;
            }
            else if (teststring.Substring(19, 1) == "A")
            {
                test9thbyte = true;
            }
            else if (teststring.Substring(19, 1) == "B")
            {
                test9thbyte = true;
            }


                Assert.IsTrue(test9thbyte, "9th byte invalid (not 8,9,A,B)");
        }
    }
}
