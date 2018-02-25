using System;
using System.Collections;
using Cosmos.TestRunner;
//using Cosmos.Compiler.Tests.Bcl.Helper;
using Cosmos.Debug.Kernel;

namespace Cosmos.Compiler.Tests.Bcl.System.Collections.Not_Generic
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

            /*
             * Got Il2CPU exception:
             * System.Exception: Original method argument $this is a reference type. Plug attribute first argument is not an argument type, nor was it marked with ObjectPointerAccessAttribute! Method: SystemObjectSystemArrayGetValueSystemInt32 Parameter: aThis
             * at Cosmos.IL2CPU.AppAssembler.GenerateMethodForward(_MethodInfo aFrom, _MethodInfo aTo) in C:\Users\fano\Documents\GitHub\Cosmos\IL2CPU\source\Cosmos.IL2CPU\AppAssembler.cs:line 1309
             * at Cosmos.IL2CPU.ILScanner.Assemble() in C:\Users\fano\Documents\GitHub\Cosmos\IL2CPU\source\Cosmos.IL2CPU\ILScanner.cs:line 951
             * at Cosmos.IL2CPU.ILScanner.Execute(MethodBase aStartMethod) in C:\Users\fano\Documents\GitHub\Cosmos\IL2CPU\source\Cosmos.IL2CPU\ILScanner.cs:line 255
             * at Cosmos.IL2CPU.CompilerEngine.Execute() in C:\Users\fano\Documents\GitHub\Cosmos\IL2CPU\source\Cosmos.IL2CPU\CompilerEngine.cs:line 168
             * Error invoking 'dotnet'.
             */
#if false
            foreach (var k in h.Keys)
            {
                Assert.IsTrue((string)k == "One" || (string)k == "Two", "Hashtable key collection returns invalid key");
            }
#endif
            Hashtable h2 = new Hashtable();

            h2.Add(42, "FortyTwo");

            Assert.IsTrue(h2.ContainsKey(42), "h2.ContainsKey() failed: existing key not found");

        }
    }
}
