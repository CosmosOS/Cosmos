//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.SingleEchoTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            try
            {
                // start by faking a b c
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, false);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, true);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x30, false);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x30, true);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x2E, false);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x2E, true);
                // enter:
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1C, false);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1C, true);

                Console.Write("Input: ");
                Assert.IsTrue(true, "Before readline");
                var input = Console.ReadLine();
                Console.Write("Text typed: ");
                Console.WriteLine(input);

                Assert.AreEqual(3, input.Length, "Length of returned string is not 3!");
                Assert.AreEqual(97, (int)input[0], "First char of returned string is not a!");
                Assert.AreEqual(98, (int)input[1], "Second char of returned string is not b!");
                Assert.AreEqual(99, (int)input[2], "Third char of returned string is not c!");

                Assert.AreEqual(3, input.Length, "Length of returned string is not 3!");
                Assert.AreEqual(97, (int)input[0], "First char of returned string is not a!");
                Assert.AreEqual(98, (int)input[1], "Second char of returned string is not b!");
                Assert.AreEqual(99, (int)input[2], "Third char of returned string is not c!");

                // now test ReadKey:

                // fake a
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, false);
                Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, true);

                var xKey = Console.ReadKey();
                Assert.IsTrue(xKey.Key == ConsoleKey.A, "ReadKey didn't return key A!");

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Caught exception: " + e.ToString());
            }
        }
    }
}
