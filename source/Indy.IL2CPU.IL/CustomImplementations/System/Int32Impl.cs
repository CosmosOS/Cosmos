using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(Int32))]
	public static class Int32Impl {
		[PlugMethod(Signature = "System_String___System_Int32_ToString____")]
		public static string ToString(ref int aThis) {
			uint xValue = (uint)aThis;
			return UInt32Impl.ToString(ref xValue);
		}
	}
}