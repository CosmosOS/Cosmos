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
                var x = new object[]
                        {
                            "The solution is: ",
                            (int)42
                        };
                var xMessage = String.Concat(x);
                Console.WriteLine(xMessage);
                Console.WriteLine("Done doing tests");
                Assert.IsTrue(true, "Dummy assertion, to test the system");
                //TestController.Completed();
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
