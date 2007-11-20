using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Win32.CustomImplementations.System {
	[Plug(Target=typeof(Environment))]
	public class EnvironmentImpl {
		[PlugMethod]
		public static string get_NewLine() {
			return "**EOL**\r\n";
		}
	}
}
