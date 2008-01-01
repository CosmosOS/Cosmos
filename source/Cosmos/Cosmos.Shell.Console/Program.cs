using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
	class Program {
		static void Main() {
			Kernel.CPU.Init ();
			Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue ();
			stages.Enqueue (new Prompter ());
			stages.Run ();

			System.Console.WriteLine ("Cosmos creation complete");
			Kernel.Interrupts.DoTest ();

			stages.Teardown ();
		}
	}
}
