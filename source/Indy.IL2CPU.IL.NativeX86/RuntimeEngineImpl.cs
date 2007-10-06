using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	public static partial class RuntimeEngineImpl {
		public static void InitializeEngine() {
			Console.Clear();
			Debug.WriteLine("Loading IDT");
			SetupInterruptDescriptorTable();
			Debug.WriteLine("Loading PICs");
			//SetupProgrammableInterruptControllers();
			//Debug.WriteLine("Kernel Booted!");
		}

		public static void FinalizeEngine() {
			
		}
	}
}
