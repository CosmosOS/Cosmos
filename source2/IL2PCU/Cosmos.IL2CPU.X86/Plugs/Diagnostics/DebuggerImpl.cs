using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.PlugsLinqTest.CustomImplementations.System.Diagnostics {
	[Plug(Target = typeof(global::System.Diagnostics.Debugger))]
	public static class DebuggerImpl {
		public static void Break() {
            // leave empty, this is handled by a special case..
		}
	}
}