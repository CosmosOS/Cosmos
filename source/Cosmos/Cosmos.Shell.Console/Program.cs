using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace Cosmos.Shell.Console {
	public class Program {

        #region Build Console
        // This contains code to launch the build console. Most users should not chagne this.
        [STAThread]
        public static void Main() {
			var xBuilder = new Builder();
			xBuilder.Build();
        }
        #endregion

        // Here is where your Cosmos code goes. This is the code that will be executed during Cosmos boot.
        // Write your code, and run. Cosmos build console will appear, select your target, and thats it!
        public static void Init() {
			Kernel.CPU.Init();

			Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
			stages.Enqueue(new Prompter());

			System.Console.Clear();
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.ForegroundColor = ConsoleColor.Red;
			System.Console.WriteLine("Cosmos Kernel. Copyright 2007-2008 The Cosmos Project.");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			System.Console.ForegroundColor = ConsoleColor.Green;
			System.Console.WriteLine("Now Booting...");

			System.Console.ForegroundColor = ConsoleColor.White;
			System.Console.WriteLine("Success.");
			Kernel.CPU.PrintTime();
			stages.Run();
			System.Console.WriteLine("Done");

			stages.Teardown();
		}
	}
}
