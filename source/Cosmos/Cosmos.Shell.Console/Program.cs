using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
    class Program {
        static void Main(string[] args) {
            Kernel.CPU.Init();
            System.Console.WriteLine("Cosmos creation complete");
        	Kernel.Interrupts.DoTest();
        	int xItem = 0;
        	int xItem2 = 5;
			int xItem3 = xItem2 / xItem;
        	System.Console.WriteLine("Line after divide by zero");
        }
    }
}
