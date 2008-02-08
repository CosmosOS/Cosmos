using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(byte))]
	public static class ByteImpl {
		//[PlugMethod(Signature = "System_String___System_Byte_ToString____")]
		public static string ToString(ref byte aThis) {
			return UInt32Impl.GetNumberString(aThis, false);
		}
	}
}