using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(IntPtr))]
	public static class IntPtrImpl {
		//[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
		public static string ToString(ref uint aThis) {
			return UInt32Impl.ToString(ref aThis);
		}
	}
}