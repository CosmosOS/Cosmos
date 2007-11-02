using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class StringImpl {
		public static unsafe int get_Length(int* aThis) {
			int* xThis = aThis;
			xThis += 3;
			xThis = (int*)*xThis;
			xThis += 2;
			return *xThis;
		}

		public static byte GetByteFromChar(char aChar) {
			return 0;
		}

		public static unsafe byte get_Chars_Metal(byte* aThis, int aIndex) {
			byte* xThis = aThis;
			xThis += 4;
			xThis += aIndex;
			return *xThis;
		}
	}
}
