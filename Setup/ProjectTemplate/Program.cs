using System;
using Cosmos.Compiler.Builder;

namespace $safeprojectname$ {
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
            
            //Boot your kernel
            new Cosmos.Sys.Boot().Execute();

            //Your custom implementation
			Console.WriteLine("Welcome to COSMOS! You just booted C# code. Please edit Program.cs to fit your needs");

            //Shutdown
            Cosmos.Sys.Deboot.ShutDown();
		}
	}
}