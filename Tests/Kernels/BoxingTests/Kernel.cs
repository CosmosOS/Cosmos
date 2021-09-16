using System;
using System.Drawing;
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
            TestBoxingChar();
            TestBoxingInt32();
            TestBoxingColorToString();
            TestBoxingStruct();

            TestController.Completed();
        }

        private void TestBoxingChar()
        {
            object xChar = 'c';

            Assert.AreEqual("c", xChar.ToString(), "Char.ToString on boxed Char doesn't work!");
            // 'c' == 0x63, and the hash code is ('c' | ('c' << 16));
            Assert.AreEqual(0x00630063, xChar.GetHashCode(), "Char.GetHashCode on boxed Char doesn't work!");
        }

        private void TestBoxingInt32()
        {
            object xNumber = 42;

            Assert.AreEqual("42", xNumber.ToString(), "Int32.ToString on boxed Int32 doesn't work!");
            Assert.AreEqual(42, xNumber.GetHashCode(), "Int32.GetHashCode on boxed Int32 doesn't work!");

            Assert.IsTrue(xNumber.Equals(42), "Int32.Equals on boxed int doesn't work!");
            Assert.IsFalse(xNumber.Equals(5), "Int32.Equals on boxed int doesn't work!");

            object xAnotherNumber = 42;

            Assert.IsTrue(Object.Equals(xNumber, xAnotherNumber), "Object.Equals doesn't work!");
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


        // Taken from https://github.com/CosmosOS/Cosmos/issues/1082
        private void TestBoxingStruct()
        {
            UTF8Encoding encoding = new UTF8Encoding();
            mDebugger.Send("Testing boxing on structs!");
            object xValBoxed = new Values("UTF-8", encoding);
            bool condition = xValBoxed.Equals(xValBoxed);
            Assert.IsTrue(condition, "Equality works for boxed structs");
        }
    }

    struct Values
    {
        public string desc;
        public Encoding encoding;

        public Values(string desc, Encoding encoding)
        {
            this.desc = desc;
            this.encoding = encoding;
        }
    }
}
