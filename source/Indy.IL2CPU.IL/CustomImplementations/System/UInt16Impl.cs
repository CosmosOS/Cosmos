using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(ushort))]
	public static class UInt16Impl {
		//[PlugMethod(Signature = "System_String_System_UInt16_ToString__System_String__")]
        public static string ToString(ref ushort aThis)
        {
			return UInt32Impl.GetNumberString(aThis, false);
		}
	}
}