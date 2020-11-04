using System;
using System.Collections;
using System.Collections.Generic;
using Sys = Cosmos.System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.TypeSystem
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting BCL tests now please wait...");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                object xString = "a";

                Assert.IsTrue(xString.GetType() == typeof(string), "GetType or typeof() isn't working on reference types!");
                Assert.IsTrue(xString is ICloneable, "isinst isn't working for interfaces  on reference types!");
                Assert.IsTrue(xString is IEnumerable<char>, "isinst isn't working for generic interfaces on reference types!");

                IComparable<int> xNumber = 3;

                Assert.IsTrue(xNumber.GetType() == typeof(int), "GetType or typeof() isn't working on value types!");
                Assert.IsTrue(xNumber is IConvertible, "isinst isn't working for interfaces on value types!");
                Assert.IsTrue(xNumber is IEquatable<int>, "isinst isn't working for generic interfaces on value types!");

                IEnumerable<string> xEnumerable = new List<string>();

                Assert.IsTrue(xEnumerable.GetType() == typeof(List<string>), "GetType or typeof() isn't working on generic reference types!");
                Assert.IsTrue(xEnumerable is IEnumerable, "isinst isn't working for interfaces on generic reference types!");
                Assert.IsTrue(xEnumerable is IList<string>, "isinst isn't working for generic interfaces on generic reference types!");

                ThreeBytes[] array = new ThreeBytes[1];
                array[0] = new ThreeBytes(17, 254, 77);
                Assert.AreEqual(array[0].A, 17, "Three byte struct in array first value read correctly");
                Assert.AreEqual(array[0].B, 254, "Three byte struct in array second value read correctly");
                var local = array[0];
                Assert.AreEqual(local.C, 77, "Three byte struct third value read correctly");

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);

                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);

                TestController.Failed();
            }
        }
    }

    public struct ThreeBytes
    {
        public byte A;
        public byte B;
        public byte C;

        public ThreeBytes(byte a, byte b, byte c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
