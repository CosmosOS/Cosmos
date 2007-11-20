using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.Win32.CustomImplementations.System {
	[Plug(Target=typeof(Environment))]
	public class EnvironmentImpl {
		[PlugMethod]
		public static string get_NewLine() {
			return "\r\n";
		}
	}
}
