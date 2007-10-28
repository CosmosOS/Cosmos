using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	public static partial class RuntimeEngineImpl {
		public static MultiBootInfoStruct mMultibootInfo;
		public static void InitializeEngine() {
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