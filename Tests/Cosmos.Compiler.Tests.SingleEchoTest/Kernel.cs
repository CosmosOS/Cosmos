using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
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
            // start by faking a b c
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, false);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1E, true);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x2E, false);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x2E, true);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x30, false);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x30, true);
            // enter:
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1C, false);
            Sys.TestingHelpers.KeyboardAddFakeScanCode(0x1C, true);

            Console.Write("Input: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
            while (true)
                ;
        }
    }
}
