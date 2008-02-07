using System;
using Cosmos.Build.Windows;

namespace KudzuTest {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
			var xBuilder = new Builder();
			xBuilder.Build();
		}
		#endregion

        static protected string StringTest() {
		    string x = "Hello";
            x = x + " world.";
            return x;
        }

        static protected string StringBuilderTest() {
			System.Console.WriteLine("Step 0");
			System.Diagnostics.Debugger.Break();
			var xSB = new System.Text.StringBuilder("Hello");
			System.Diagnostics.Debugger.Break();
			Console.WriteLine("SB step one succeeded");
			System.Diagnostics.Debugger.Break();
			xSB.Append(" world.");
			System.Diagnostics.Debugger.Break();
			Console.WriteLine("SB step two succeeded");
			System.Diagnostics.Debugger.Break();
			return xSB.ToString();
        }

		// Main entry point of the kernel
		public static void Init() {
            Cosmos.Kernel.Boot.Default();
//			System.Diagnostics.Debugger.Break();
            Console.WriteLine("Boot complete");

            Tests.DoAll();

            //Cosmos.Kernel.Temp.Kudzu.PCI.Test();

            Console.WriteLine("Shell complete");
            while (true)
				;
		}
	}
}
