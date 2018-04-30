using System;
using System.Collections;
using Cosmos.TestRunner;
//using Cosmos.Compiler.Tests.Bcl.Helper;
using Cosmos.Debug.Kernel;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections
{
    class HashtableTest
    {
        private static Debugger myDebugger = new Debugger("System", "HashtableTest");

        public static void Execute()
        {
            var h = new Hashtable();
            Assert.IsTrue(h != null, "Hashtable ctor returns but h is null");

            h.Add("One", "One");
            //h.Add(42, "Test");

            Assert.IsTrue(h.ContainsKey("One"), "Hashtable.ContainsKey() failed: existing key not found");
            Assert.IsFalse(h.ContainsKey("Two"), "Hashtable.ContainsKey() failed: not existing key not found");

            Assert.IsTrue((string)h["One"] == "One", "Hashtable indexer failed: existing value not found");
            Assert.IsTrue(h["Two"] == null, "Hashtable indexer failed: not existing value not found");

            /* The indexer written in this way should be the same thing of Add("Two", "Two") */
            h["Two"] = "Two";

            Assert.IsTrue((string)h["Two"] == "Two", "Hashtable indexer failed: existing value (II) not found");

            Assert.IsTrue(h.Count == 2, "Hashtable Count failed: value != 2");

            foreach (var k in h.Keys)
            {
                Assert.IsTrue((string)k == "One" || (string)k == "Two", "Hashtable key collection returns invalid keys");
            }

            /* This continues to not work: 42 is not a key! Why? */ 
#if false
            Hashtable h2 = new Hashtable();

            h2.Add(42, "FortyTwo");

            Assert.IsTrue(h2.ContainsKey((int)42), "h2.ContainsKey() failed: existing key not found");
#endif
        }
    }
}
