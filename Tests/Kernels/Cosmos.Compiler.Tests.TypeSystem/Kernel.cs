﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sys = Cosmos.System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.TypeSystem
{
    public class Kernel : Sys.Kernel
    {
        static int test = 0;
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
