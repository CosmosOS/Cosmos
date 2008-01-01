using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
	class Program {
		static void Main() {
			Kernel.CPU.Init();
			System.Console.WriteLine("Cosmos creation complete");
			Kernel.Interrupts.DoTest();
		}
	}
}
