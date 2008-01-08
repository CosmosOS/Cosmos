using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace Cosmos.Shell.Console {
	public class Program {
        public static void Main() {
			var xBuilder = new Builder();
			xBuilder.Build(Builder.Target.QEMU_GDB);
        }
        
        public static void Init() {
			Kernel.CPU.Init();

			Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
			stages.Enqueue(new Prompter());

			System.Console.Clear();
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.ForegroundColor = ConsoleColor.Red;
			System.Console.WriteLine("Cosmos Kernel. Copyright 2007-2008 The Cosmos Project.");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			System.Console.ForegroundColor = ConsoleColor.Green;
			System.Console.WriteLine("Now Booting...");

			System.Console.ForegroundColor = ConsoleColor.White;
			System.Console.WriteLine("Success.");
			Kernel.CPU.PrintTime();
			stages.Run();
			System.Console.WriteLine("Done");

			while (true)
			    ;


			stages.Teardown();
		}
	}
}
