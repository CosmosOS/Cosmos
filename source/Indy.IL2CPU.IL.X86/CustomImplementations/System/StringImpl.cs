using System;
using System.Linq;
using Indy.IL2CPU.Plugs;

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

		[PlugMethod(Signature = "System_Char___System_String_get_Chars___System_Int32___")]
		public static unsafe ushort get_Chars(uint* aThis, int aIndex) {
			uint* xThis = aThis;
			xThis += 3;
			xThis = (uint*)*xThis;
			xThis += 4;
			ushort* aBytes = (ushort*)xThis;
			aBytes += aIndex;
			return *aBytes;
		}

		[PlugMethod(Signature = "System_Void___System_String__cctor____", InNormalMode=false)]
		public static void cctor() {
		}
	}

	[Plug(Target = typeof(Indy.IL2CPU.CustomImplementation.System.StringImpl))]
	public static class StringImpl2 {
		[PlugMethod(Signature = "System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_String___")]
		public static unsafe uint GetStorage(uint* aStringPtr) {
			uint* xThis = aStringPtr;
			xThis += 3;
			xThis = (uint*)*xThis;
			xThis += 4;
			return (uint)xThis;
		}

		public static void FakeMethod() {
			char[] xThis = null;
			CustomImplementation.System.StringImpl.Ctor(null, ref xThis, null);
		}
	}
}
