using System;
using Indy.IL2CPU.NativeX86.Utilities.BIOS;

namespace Cosmos.Kernel {
	class ConsoleDrv {
		public static void Main() {
			Console.WriteLine("This is Indy OS...");
			//int xDivider = 0;
			Console.WriteLine("Try Interrupts now");
			System.Diagnostics.Debugger.Break();
			TestIDT();
		}

		public static void TestIDT() {
		}
	}
}
