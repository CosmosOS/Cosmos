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

		// Main entry point of the kernel
		public static void Init() {
            Cosmos.Kernel.Boot.Default();
//			System.Diagnostics.Debugger.Break();
            Console.WriteLine("Boot complete");

            //Tests.Do("String Concatenation", Tests.StringConcat);
            Console.WriteLine("String test");
            Console.WriteLine("  " + Tests.StringConcat());
            Console.WriteLine();

            Console.WriteLine("StringBuilder test");
            //Console.WriteLine("  " + Tests.StringBuilder());
            Console.WriteLine();

            //Cosmos.Kernel.Temp.Kudzu.PCI.Test();

            Console.WriteLine("Shell complete");
            while (true)
				;
		}
	}
}