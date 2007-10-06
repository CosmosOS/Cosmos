using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	public static partial class RuntimeEngineImpl {
		public static void InitializeEngine() {
			DebugHelper.MakeWritingPossible();
			Console.Clear();
			Debug.WriteLine("Slowing down PIT");
			PIT_SetSlowest();
			Debug.WriteLine("Loading IDT");
			SetupInterruptDescriptorTable();
			Debug.WriteLine("Loading PICs");
			SetupProgrammableInterruptControllers();
			Debug.WriteLine("Kernel Booted");
		}

		public static void FinalizeEngine() {
			Debug.WriteLine("Engine Shut down done.");
		}
	}
}
