using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(char))]
	public static class CharImpl {
		public static string ToString(char aThis) {
			ushort xValue = aThis;
			return UInt16Impl.ToString(xValue);
		}
	}
}
