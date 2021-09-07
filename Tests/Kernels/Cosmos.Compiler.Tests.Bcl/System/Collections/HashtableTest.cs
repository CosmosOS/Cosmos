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

            foreach (DictionaryEntry k in h)
            {
                Assert.IsTrue((string)k.Key == "One" || (string)k.Key == "Two", "h enumeration returns invalid keys key");
                Assert.IsTrue((string)k.Value == "One" || (string)k.Value == "Two", "h enumeration returns invalid values");
            }

            Hashtable h2 = new Hashtable();

            h2.Add(42, "FortyTwo");

            Assert.IsTrue(h2.ContainsKey(42), "h2.ContainsKey() failed: existing key not found");
            Assert.IsFalse(h2.ContainsKey(43), "h2.ContainsKey() failed: not existing key found");

            Assert.IsTrue((string)h2[42] == "FortyTwo", "h2 indexer failed: existing value not found");
            Assert.IsTrue((string)h2[43] == null, "h2 indexer failed: not existing value found");

            /* The indexer written in this way should be the same thing of Add("Two", "Two") */
            h2[43] = "FortyThree";

            Assert.IsTrue((string)h2[43] == "FortyThree", "h2 indexer failed: existing value (II) not found");

            Assert.IsTrue(h2.Count == 2, "h2 Count failed: value != 2"); 

            foreach (var k in h2.Keys)
            {
                Assert.IsTrue((int)k == 42 || (int)k == 43, "h2 key collection returns invalid keys");
            }

            foreach (DictionaryEntry k in h2)
            {
                Assert.IsTrue((int)k.Key == 42  || (int)k.Key == 43, "h2 enumeration returns invalid keys key");
                Assert.IsTrue((string)k.Value == "FortyTwo" || (string)k.Value == "FortyThree", "h2 enumeration returns invalid values");
            }

            Hashtable h3 = new Hashtable();

            h3.Add('A', 41);
            Assert.IsTrue(h3.ContainsKey('A'), "h3.ContainsKey() failed: existing key not found");

            Assert.IsTrue((int)h3['A'] == 41, "h3 indexer failed: existing value not found");

            Assert.IsTrue(h3['B'] == null, "h3 indexer failed: not existing value found");

            /* The indexer written in this way should be the same thing of Add("Two", "Two") */
            h3['B'] = 42;

            Assert.IsTrue((int)h3['B'] == 42, "h3 indexer failed: existing value (II) not found");

            Assert.IsTrue(h3.Count == 2, "h3 Count failed: value != 2");

            foreach (var k in h3.Keys)
            {
                Assert.IsTrue((char)k == 'A' || (char)k == 'B', "h3 key collection returns invalid keys");
            }

            foreach (DictionaryEntry k in h3)
            {
                Assert.IsTrue((char)k.Key == 'A' || (char)k.Key == 'B', "h3 enumeration returns invalid keys");
                Assert.IsTrue((int)k.Value == 41 || (int)k.Value == 42, "h3 enumeration returns invalid values");
            }

            
        }
    }
}
