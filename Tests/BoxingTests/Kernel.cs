using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace BoxingTests
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
        }

        protected override void Run()
        {
            Assert.IsTrue(TestBoxingCharToString(), "Boxing char to string test failed.");
            Assert.IsTrue(TestBoxingCharArrayToString(), "Boxing char[] to string test failed.");
            TestController.Completed();
        }

        private bool TestBoxingCharToString()
        {
            try
            {
                char xC = 'c';
                string xS = xC.ToString();
                return (xS[0] == xC);
            }
            catch (Exception E)
            {
                mDebugger.SendError("TestBoxingCharToString", E.Message);
                return false;
            }
        }

        private bool TestBoxingCharArrayToString()
        {
            try
            {
                char[] xC = {'c'};
                string xS = xC.ToString();
                return (xS[0] == xC[0]);
            }
            catch (Exception E)
            {
                mDebugger.SendError("TestBoxingCharArrayToString", E.Message);
                return false;
            }
        }
    }
}
