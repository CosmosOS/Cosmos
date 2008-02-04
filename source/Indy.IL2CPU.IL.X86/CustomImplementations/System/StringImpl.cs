using System;
using System.Linq;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(String))]
	public static class StringImpl {
		public static unsafe int get_Length(int* aThis, [FieldAccess(Name = "$$Storage$$")]ref char[] aStorage) {
			return aStorage.Length;
		}

		public static unsafe char get_Chars(uint* aThis, [FieldAccess(Name = "$$Storage$$")]ref char[] aStorage,int aIndex) {
			return aStorage[aIndex];
		}

		public static void cctor() {
		}
	}

	[Plug(Target = typeof(Indy.IL2CPU.CustomImplementation.System.StringImpl))]
	public static class StringImpl2 {
		public static unsafe uint GetStorage(uint* aStringPtr, [FieldAccess(Name = "$$Storage$$")]ref uint aStorage) {
			return aStorage;
		}

		public static void FakeMethod() {
			char[] xThis = null;
			CustomImplementation.System.StringImpl.Ctor(null, ref xThis, null);
		}
	}
}
