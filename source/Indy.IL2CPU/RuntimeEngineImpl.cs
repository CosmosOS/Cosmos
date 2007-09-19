using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static partial class RuntimeEngine {
		public static void InitializeApplication() {
			// do initialization of all runtime services, like heap and GC..
			StartupHeap();
			
		}

		public static void FinalizeApplication(uint aExitCode) {
			// finalize all runtime services, like heap and gc
			ShutdownHeap();
			PInvokes.Kernel32_ExitProcess(aExitCode);
		}
	}
}
