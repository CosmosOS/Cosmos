using System;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System {
    [Plug(Target=typeof(EventHandler))]
	public static class EventHandlerImpl {
    //public static void ctor(uint aThis, object aObject, IntPtr aMethod, [FieldAccess(Name = "System.Object System.Delegate._target")] ref object aFldTarget, [FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref int aFldMethod) {
    //  // move forward 8 bytes
    //  aFldTarget = aObject;
    //  aFldMethod = aMethod.ToInt32();
    //}

		public static bool Equals(EventHandler aThis, object aThat) {
            // todo: implement EventHandler.Equals(object)
		    return false;
		}
	}
}
