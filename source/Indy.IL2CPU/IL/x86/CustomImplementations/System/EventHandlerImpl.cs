using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
    [Plug(Target=typeof(EventHandler))]
	public static class EventHandlerImpl {
		public static void Ctor(uint aThis, uint aObject, uint aMethod, [FieldAccess(Name = "System.Object System.Delegate._target")] ref uint aFldTarget, [FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref uint aFldMethod) {
			// move forward 8 bytes
			aFldTarget = aObject;
			aFldMethod = aMethod;
		}							
		
		public static bool Equals(EventHandler aThis, object aThat) {
            // todo: implement EventHandler.Equals(object)
		    return false;
		}
	}
}
