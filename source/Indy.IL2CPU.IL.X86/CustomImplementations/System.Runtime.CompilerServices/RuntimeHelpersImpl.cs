using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Runtime.CompilerServices {
	[Plug(Target=typeof(RuntimeHelpers))]
	public static class RuntimeHelpersImpl {
		[PlugMethod(Signature="System_Void___System_Runtime_CompilerServices_RuntimeHelpers__cctor____")]
		public static void CCtor() {
			//todo: do something
		}
	}
}