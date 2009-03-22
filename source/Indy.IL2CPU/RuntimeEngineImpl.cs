using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static partial class RuntimeEngine {
		public static void InitializeApplication() {
			//Console.WriteLine("Initial Setup");
			// do initialization of all runtime services, like heap and GC..
			InitializeEngine();
			Heap_Initialize();
		}

		public static void InitializeEngine() {
            //int xTest = 5;
			// for replacement by the architecture
		}

		public static void FinalizeEngine() {
			// for replacement by the architecture
		}

		public static void FinalizeApplication(int aExitCode) {
			// finalize all runtime services, like heap and gc
			//Heap_Shutdown();
			FinalizeEngine();
			ExitProcess(aExitCode);			
		}

		public static void ExitProcess(int aExitCode) {
		}
	}
}
