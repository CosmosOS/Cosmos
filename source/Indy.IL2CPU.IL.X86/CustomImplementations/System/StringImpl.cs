using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(String))]
	public static class StringImpl {
		[PlugMethod(Signature = "System_Int32___System_String_get_Length____")]
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

		public static unsafe byte get_Chars_Metal(uint* aThis, int aIndex) {
			uint* xThis = aThis;
			xThis += 3;
			xThis = (uint*)*xThis;
			xThis += 3;
			byte* aBytes = (byte*)xThis;
			aBytes += aIndex;
			return *aBytes;
		}

		public static unsafe ushort get_Chars_Normal(uint* aThis, int aIndex) {
			uint* xThis = aThis;
			xThis += 3;
			xThis = (uint*)*xThis;
			xThis += 3;
			ushort* aBytes = (ushort*)xThis;
			aBytes += aIndex;
			return *aBytes;
		}
	}
}
