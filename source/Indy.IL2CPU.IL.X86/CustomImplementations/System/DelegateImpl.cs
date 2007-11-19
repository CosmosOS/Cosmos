using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(Delegate))]
	public static class DelegateImpl {
		[PlugMethod(Signature = "System_IntPtr___System_Delegate_GetInvokeMethod____")]
		public static unsafe uint GetInvokeMethod(uint* aThis) {
			return *(aThis + 2);
		}

		public static void DoSomethingWithDelegate(Delegate aDelegate) {
			// fake method to have the type Delegate referenced by the assembly
			object o = aDelegate.Target;
			object o2 = o;

		}
	}
}