using System;
using Cosmos.Build.Windows;

namespace CosmosBoot {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
            BuildUI.Run();
        }
		#endregion

		// Main entry point of the kernel
		public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            Console.WriteLine("Congratulations! You just booted C# code.");
            Console.WriteLine("Edit Program.cs to create your own Operating System.");
            while (true) {
                // Put your code here
            }
		}
	}
}