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
	}
}
