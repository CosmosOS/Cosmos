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
            Console.WriteLine("Hello " + 7.ToString());
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
        }
    }
}
