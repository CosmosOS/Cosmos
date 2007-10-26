using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static unsafe class DelegateImpl {
		[MethodAlias(Name = "System.Boolean System.Delegate.InternalEqualTypes(System.Object,System.Object)")]
		public static bool InternalEqualTypes(uint* aDelegate1, uint* aDelegate2) {
			return *aDelegate1 == *aDelegate2;
		}
	}
}
