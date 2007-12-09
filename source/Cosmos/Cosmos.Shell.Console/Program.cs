using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
	class Program {
		static void Main() {
			Kernel.CPU.Init();
			System.Console.WriteLine("Cosmos creation complete");
			Kernel.Interrupts.DoTest();
			int myInt = 0;
			int myInt2 = 2 / myInt;
			
		}
	}
}
