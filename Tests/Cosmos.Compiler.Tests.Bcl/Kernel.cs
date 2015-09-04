using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Bcl
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            System.Collections.Generic.ListTest.Execute();
            System.Collections.Generic.QueueTest.Execute();

            TestController.Completed();
        }
    }
}
