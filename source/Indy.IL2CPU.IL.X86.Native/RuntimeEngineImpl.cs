using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
//using Cosmos.Kernel.Boot.Glue;

namespace Indy.IL2CPU.IL.X86.Native {
	[Plug(Target = typeof(RuntimeEngine))]
	public static partial class RuntimeEngineImpl {
		//private static void LoadBootInformation(ref BootInformationStruct aBootInfo) {
		// implemented using Gluemethods
		//}

		public static void InitializeEngine() {
			Console.Clear();
			//			Debug.WriteLine("Loading GDT");
			//			SetupGDT();
			//			Debug.WriteLine("Slowing down PIT");
			//			PIT_SetSlowest();
			//			Debug.WriteLine("Loading IDT");
			//			SetupInterruptDescriptorTable();
			//			Debug.WriteLine("Loading PICs");
			//			SetupProgrammableInterruptControllers();
			//			Debug.WriteLine("Kernel Booted");
		}

		public static void FinalizeEngine() {
			//Debug.WriteLine("Engine Shut down done.");
		}
	}
}