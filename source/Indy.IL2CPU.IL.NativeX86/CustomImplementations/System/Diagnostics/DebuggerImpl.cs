using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System.Diagnostics {
	public static class DebuggerImpl {
		public static unsafe void Break() {
			byte* xScreenPtr = (byte*)0xB8000;
			byte xTemp = *xScreenPtr;
		}
	}
}