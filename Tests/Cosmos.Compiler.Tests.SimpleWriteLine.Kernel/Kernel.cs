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
            var xMessage = "Cosmos booted successfully. Type a line of text to get it echoed back.";
            Console.WriteLine(xMessage);
            Debugger.DoSend("After writeline");
        }

        protected override void Run()
        {
            Debugger.DoSend("In Run");
            try
            {
                Console.WriteLine("Started correctly!");
                Assert.IsTrue(true, "Dummy assertion, to test the system");
                Console.WriteLine("After assertion");
                TestController.Completed();
                while (true)
                    ;
            }
            catch(Exception E)
            {
                Console.WriteLine("Exception");
                Console.WriteLine(E.ToString());
            }
        }
    }
}
