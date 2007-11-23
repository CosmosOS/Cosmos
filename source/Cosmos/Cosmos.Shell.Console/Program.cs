using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
    class Program {
        static void Main(string[] args) {
            Kernel.CPU.Init();
            System.Console.WriteLine("Cosmos creation complete");
        	Kernel.Interrupts.DoTest();
        }
    }
}
