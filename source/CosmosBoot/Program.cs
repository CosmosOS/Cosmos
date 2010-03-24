using System;
using Cosmos.Compiler.Builder;
using S = Cosmos.Hardware.TextScreen;
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

            Console.WriteLine("Cosmos Booted Succesfully!");
            while (true)
                ;

            Cosmos.Sys.Deboot.ShutDown();
		}
	}
}