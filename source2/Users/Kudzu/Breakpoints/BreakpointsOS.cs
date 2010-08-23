using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;

namespace BreakpointsKernel {
    public class BreakpointsOS : Sys.Kernel {
        public BreakpointsOS() {
            ClearScreen = false;
        }

        protected override void BeforeRun() {
            UInt32 x = 0x000010E1;
            x = x & 0xFFFFFFFC;
            Console.WriteLine(x.ToHex());

            Console.WriteLine("Hello " + 6.ToString());
            //Debugger.Send("Hello from Cosmos!");
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            //Debugger.Break();
            Console.WriteLine("Test");
        }

        protected override void Run() {
            Console.Write("Input: ");
            string xResult = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(xResult);
                                                  
            var xTempDebugger = new Debugger("app", "test");
            xTempDebugger.Send("Hello, World");
        }
    }
}
