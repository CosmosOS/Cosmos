using System;
using Cosmos.Build.Windows;

namespace CosmosBoot {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
			var xBuilder = new Builder();
			xBuilder.Build();
		}
		#endregion

		// Main entry point of the kernel
		public static void Init() {
            Cosmos.Kernel.Boot.Default();
			Console.WriteLine("Welcome! You just booted C# code. Please edit Program.cs to fit your needs");
			while (true)
				;
		}
	}
}