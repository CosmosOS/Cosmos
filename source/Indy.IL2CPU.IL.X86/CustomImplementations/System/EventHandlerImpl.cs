using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static unsafe class EventHandlerImpl {
		public static void Ctor(uint* aThis, uint aObject, uint aMethod) {
			// move forward 8 bytes
			uint* xThis = aThis;
			aThis += 2;
			*aThis = aObject;
			aThis += 1;
			*aThis = aMethod;
		}

		public static void MulticastInvoke() {
			// do nothing
			uint* x = (uint*)0;
			uint y = *x;
		}

		public static uint GetInvokeMethod(uint* aThis) {
			return *(aThis + 2);
		}
	}
}
