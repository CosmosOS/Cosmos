using System;
using System.Collections;
using System.Collections.Generic;
using Sys = Cosmos.System;

using Cosmos.TestRunner;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.Compiler.Tests.TypeSystem
{
    class A
    {

    }

    class B : A
    {

    }

    public class Kernel : Sys.Kernel
    {
        static int test = 0;
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting Type tests now please wait...");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                object xString = "a";
                string xString2 = "b";

                Assert.IsTrue(xString.GetType() == typeof(string), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xString.GetType() == xString2.GetType(), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xString is ICloneable, "isinst works for interfaces on reference types!");
                Assert.IsTrue(xString is IEnumerable<char>, "isinst works for generic interfaces on reference types!");
                Assert.IsFalse(xString.GetType().IsValueType, "IsValueType works for reference types!");

                IComparable<int> xNumber = 3;

                Assert.IsTrue(xNumber.GetType() == typeof(int), "GetType or typeof() works for value types!");
                Assert.IsTrue(xNumber is IConvertible, "isinst works for interfaces on value types!");
                Assert.IsTrue(xNumber is IEquatable<int>, "isinst works for generic interfaces on value types!");

                IEnumerable<string> xEnumerable = new List<string>();

                Assert.IsTrue(xEnumerable.GetType() == typeof(List<string>), "GetType or typeof() works for reference types!");
                Assert.IsTrue(xEnumerable is IEnumerable, "isinst works for interfaces on generic reference types!");
                Assert.IsTrue(xEnumerable is IList<string>, "isinst works for generic interfaces on generic reference types!");

                B b = new B();
                Assert.IsTrue(b.GetType() == typeof(B), "GetType or typeof() works for custom types!");

                Type baseType = b.GetType().BaseType;
                Type objectType = typeof(A);
                Assert.IsTrue(baseType == objectType, "BaseType works for custom reference types!");
                Assert.IsTrue(b.GetType().BaseType == new B().GetType().BaseType, "BaseType works for custom reference types!");
                Assert.IsTrue(b.GetType().IsSubclassOf(typeof(A)), "IsSubClassOf works for custom reference types!");

                byte xByte = 1;
                Assert.IsTrue(xByte.GetType() == typeof(byte), "GetType or typeof() works for value types!");
                Assert.IsTrue(xByte.GetType().IsSubclassOf(typeof(ValueType)), "IsSubClassOf works for value types!");
                Assert.IsTrue(xByte.GetType().IsValueType, "IsValueType works for value types!");

                Action a = () => { };
                Action<int> a1 = (i) => test++;
                Assert.IsTrue(a != null , "Anonymous type for action is created correctly");
                Assert.IsTrue(a1 != null, "Anonymous type for action<int> is created correctly");

                var c = new { i = 1, n = "Test" };
                Assert.IsTrue(c != null, "Anonymous types are created correctly");
                Assert.IsTrue(c.i == 1 && c.n == "Test", "Anonymous types have correct values");

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
}
