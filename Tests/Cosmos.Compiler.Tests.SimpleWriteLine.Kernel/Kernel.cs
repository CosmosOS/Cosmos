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
                object x = 42;
                Console.WriteLine(x.ToString());
                Console.WriteLine("Done doing tests");

                Assert.IsTrue(true, "Dummy assertion, to test the system");

                Debugger.DoSend("Test TryFinally now");
                TestTryFinally.Execute();

                Assert.IsTrue(InterruptsEnabled, "Interrupts are not enabled!");

                var xTempString = new String('a', 4);
                Assert.AreEqual(4, xTempString.Length, "Dynamic string has wrong length!");
                Assert.AreEqual(97, (int)xTempString[0], "First character of dynamic string is wrong!");

                TestController.Completed();
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception");
                Console.WriteLine(E.ToString());
            }
        }
    }
}
