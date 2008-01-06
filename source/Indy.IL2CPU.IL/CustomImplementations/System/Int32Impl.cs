using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(Int32))]
	public static class Int32Impl {
		public static string ToString(int aThis) {
			return UInt32Impl.ToString((uint)aThis);
		}
	}
}