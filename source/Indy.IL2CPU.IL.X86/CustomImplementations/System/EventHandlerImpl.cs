using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static unsafe class EventHandlerImpl {
		public static void Ctor(uint* aThis, uint aObject, uint aMethod, [FieldAccess(Name = "System.Object _target")] ref uint aFldTarget, [FieldAccess(Name = "IntPtr _methodPtr")] ref uint aFldMethod) {
			//// move forward 8 bytes
			global::System.Diagnostics.Debugger.Break();
			aFldTarget = aObject;
			aFldMethod = aMethod;
		}
	}
}
