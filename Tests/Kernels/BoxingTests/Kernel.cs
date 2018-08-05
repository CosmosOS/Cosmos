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
            TestBoxingChar();
            TestBoxingInt();
            TestBoxingColorToString();

            TestController.Completed();
        }

        private void TestBoxingChar()
        {
            object xChar = 'c';

            Assert.IsTrue(xChar.ToString() == "c", "Char.ToString on boxed Char doesn't work!");
            // 'c' == 0x63, and the hash code is ('c' | ('c' << 16));
            Assert.IsTrue(xChar.GetHashCode() == 0x00630063, "Char.GetHashCode on boxed Char doesn't work!");
        }

        private void TestBoxingInt()
        {
            object xNumber = 42;

            Assert.IsTrue(xNumber.ToString() == "42", "Int32.ToString on boxed Int32 doesn't work!");
            Assert.IsTrue(xNumber.GetHashCode() == 42, "Int32.GetHashCode on boxed Int32 doesn't work!");
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
        private void TestBoxingColorToString()
        {
            object xColor = Color.Blue;
            Assert.IsTrue(xColor.ToString() == "Color [Blue]", "Color.ToString doesn't work on boxed Color!");
        }
    }
}
