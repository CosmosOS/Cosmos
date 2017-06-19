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

           Sys.Guid trueGuid = Sys.Guid.NewGuid();
           
            string stringGuid = trueGuid.ToString();

            Assert.IsTrue((stringGuid.Length == 36), "invalid Length");

            Assert.IsTrue((stringGuid.Substring(14, 1) == "4"), "7th byte invalid (not 4)");

            bool test9thbyte = false;

            if (stringGuid.Substring(19, 1) == "8")
            {
                test9thbyte = true;
            }
            else if (stringGuid.Substring(19, 1) == "9")
            {
                test9thbyte = true;
            }
            else if (stringGuid.Substring(19, 1) == "A")
            {
                test9thbyte = true;
            }
            else if (stringGuid.Substring(19, 1) == "B")
            {
                test9thbyte = true;
            }


                Assert.IsTrue(test9thbyte, "9th byte invalid (not 8,9,A,B)");
        }
    }
}
