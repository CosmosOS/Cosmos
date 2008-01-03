using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
	class Program {
		static void Main() {
			Kernel.CPU.Init();

			Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
			//stages.Enqueue(new Prompter());

			//System.Console.Clear();
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.ForegroundColor = ConsoleColor.Red;
			System.Console.WriteLine("Cosmos Kernel. Copyright 2008 The Cosmos Project.");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			System.Console.ForegroundColor = ConsoleColor.Green;
			System.Console.WriteLine("Now Booting...");

			System.Console.ForegroundColor = ConsoleColor.White;
			stages.Run();
			System.Console.WriteLine("Success.");
			Kernel.CPU.PrintTime();
			System.Console.Write("Testing Prompter now:" );
			Prompter p = new Prompter();

			System.Console.WriteLine("Done");

			while (true)
				;


			stages.Teardown();
		}
	}
}
