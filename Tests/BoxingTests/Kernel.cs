using System;
using System.Drawing;

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
            Assert.IsTrue(TestBoxingIntToString(), "Boxing int to string test failed.");
#if !NETSTANDARD1_5
            Assert.IsTrue(TestBoxingColorToString(), "Boxing of Color to string test failed.");
#endif

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

        /* This test fails with "Object.ToString() not yet implemented" written in the Console */
        private bool TestBoxingCharArrayToString()
        {
            try
            {
                char[] xC = { 'c' };
                string xS = xC.ToString();
                return (xS[0] == xC[0]);
            }
            catch (Exception E)
            {
                mDebugger.SendError("TestBoxingCharArrayToString", E.Message);
                return false;
            }
        }

        private bool TestBoxingIntToString()
        {
            try
            {
                object boxMe;
                int anInt = 42;

                boxMe = anInt;

                return (boxMe.ToString() == "42");
            }
            catch (Exception E)
            {
                mDebugger.SendError("TestBoxingIntToString", E.Message);
                return false;
            }
        }

        /* TODO add other tests:
         * - a simple stucture with fixed layout (for example with the integers and a ToString() method implemented)
         * - the structure of above but without layout set (that is sequential should be automatically taken by compiler)
         * - a structure with auto layout
         * - a strucuture with the packing attribute set with not a default value used
         */

        /*
         * The struct Color of System.Drawging has really a weird layout that make so that the runtime should create
         * padding between the fields to align the size of the structure to 4 bytes.
         * Cosmos ignores this and put no padding / writes the struct wrongly in memory and then when it should be
         * boxed garbage is copied instead of the structure itself!
         */
#if !NETSTANDARD1_5
        private bool TestBoxingColorToString()
        {
            try
            {
                object boxMe;
                Color color = Color.Blue;

                boxMe = color;

                return (boxMe.ToString() == "Color[Blue]");
            }
            catch (Exception E)
            {
                mDebugger.SendError("TestBoxingIntToString", E.Message);
                return false;
            }
        }
#endif
    }
}
