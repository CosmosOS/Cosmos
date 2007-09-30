using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static partial class RuntimeEngine {
		public static void InitializeApplication() {
			// do initialization of all runtime services, like heap and GC..
			Heap_Initialize();
		}

		public static void FinalizeApplication(int aExitCode) {
			// finalize all runtime services, like heap and gc
			Heap_Shutdown();
			ExitProcess(aExitCode);			
		}

		public static void ExitProcess(int aExitCode) {
		}
	}
}
