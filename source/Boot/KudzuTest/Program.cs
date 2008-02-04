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
			//var xSB = new System.Text.StringBuilder("Hello");
			//xSB.Append(" world.");
			//return xSB.ToString();
			return "";
        }

		// Main entry point of the kernel
		public static void Init() {
            Cosmos.Kernel.Boot.Default();
//			System.Diagnostics.Debugger.Break();
            Console.WriteLine("Boot complete");

            // Matthijs - uncomment these tests to show the errors
            // I described to you.
            //Console.WriteLine("String test");
            //Console.WriteLine(StringTest());
            //Console.WriteLine("StringBuilder test");
            //Console.WriteLine(StringBuilderTest());

            //Cosmos.Kernel.Temp.Kudzu.PCI.Test();

            Console.WriteLine("Shell complete");
            while (true)
				;
		}
	}
}