using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
	class Program {
		static void Main() {
			Kernel.Stages.Initialize ();

			System.Console.WriteLine ("Cosmos creation complete");
			Kernel.Interrupts.DoTest ();

			Kernel.Stages.Teardown ();
		}
	}
}
