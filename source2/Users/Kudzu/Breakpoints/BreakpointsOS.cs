using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Debug.Kernel;

namespace BreakpointsKernel {
    public class BreakpointsOS : Sys.Kernel {
        protected override void BeforeRun() {
            Console.WriteLine("Test");
            //Debugger.Send("Hello from Cosmos!");
            Console.WriteLine("4 Cosmos booted successfully. Type a line of text to get it echoed back.");
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
