using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.SimpleWriteLine.Kernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            new Debugger("User", "test").SendNumber(0x01020304);
            Console.WriteLine("Started correctly!");
            Assert.IsTrue(true, "Dummy assertion, to test the system");
            Console.WriteLine("After assertion");
            TestController.Completed();

        }
    }
}
