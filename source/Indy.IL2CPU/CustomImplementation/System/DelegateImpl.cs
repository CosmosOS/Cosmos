using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
	[Plug(Target = typeof(Delegate))]
	public static unsafe class DelegateImpl {
		[PlugMethod(Signature = "System_Boolean__System_Delegate_InternalEqualTypes_System_Object__System_Object_")]
		public static bool InternalEqualTypes(uint* aDelegate1, uint* aDelegate2) {
			return *aDelegate1 == *aDelegate2;
		}
	}
}
