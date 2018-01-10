using System;
using System.Collections;
using Cosmos.TestRunner;
using Cosmos.Compiler.Tests.Bcl.Helper;
using Cosmos.Debug.Kernel;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Not_Generic
{
    class HashtableTest
    {
        public static void Execute()
        {
            var h = new Hashtable();
            h.Add(42, "Test");

            Assert.IsTrue(h.ContainsKey(42), "Hashtable.ContainsKey() failed: existing key not found");
            Assert.IsFalse(h.ContainsKey(24), "Hashtable.ContainsKey() failed: not existing key not found");

            /* This really requires Thread.Sleep() to work? */
            //Assert.IsTrue((string)h[42] == "Test", "Hashtable indexer not working");

        }
    }
}
