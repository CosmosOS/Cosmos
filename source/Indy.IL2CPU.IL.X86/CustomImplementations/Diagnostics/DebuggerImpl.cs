using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86LinqTest.CustomImplementations.System.Diagnostics {
	[Plug(Target = typeof(global::System.Diagnostics.Debugger))]
	public static class DebuggerImpl {
		[PlugMethod(MethodAssembler = typeof(BreakAssembler))]
		public static void Break() {
		}
	}
}