using System;
using System.Linq;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(String))]
	public static class StringImpl {
		[MethodAlias(Name = "System.Void System.String..ctor(System.Char[],System.Int32,System.Int32)")]
		public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength, [FieldAccess(Name="")]ref int aStringLength) {
			Char[] newChars = new Char[aLength];
			Array.Copy(aChars, aStartIndex, newChars, 0, aLength);
			aStorage = newChars;
		}

		[PlugMethod(Signature = "System_Void__System_String__ctor_System_Char___")]
		public static void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")] ref Char[] aStorage, Char[] aChars) {
			aStorage = aChars;
		}

		public static unsafe int get_Length(int* aThis, [FieldAccess(Name = "$$Storage$$")]ref char[] aStorage) {
			return aStorage.Length;
		}

		public static unsafe char get_Chars(uint* aThis, [FieldAccess(Name = "$$Storage$$")]ref char[] aStorage,int aIndex) {
			return aStorage[aIndex];
		}

		public static unsafe string GetStringForStringBuilder(string value, int startIndex, int length, int capacity) {
			//string str = FastAllocateString(capacity);
			//if (value.Length == 0x0) {
			//    str.SetLength(0x0);
			//    return str;
			//}
			//fixed (char* chRef = &str.m_firstChar) {
			//    fixed (char* chRef2 = &value.m_firstChar) {
			//        wstrcpy(chRef, chRef2 + startIndex, length);
			//    }
			//}
			//str.SetLength(length);
			//return str;
			return null;
		}

		public static void cctor() {
		}
	}

	[Plug(Target = typeof(Indy.IL2CPU.CustomImplementation.System.StringImpl))]
	public static class StringImpl2 {
		public static unsafe uint GetStorage(uint* aStringPtr, [FieldAccess(Name = "$$Storage$$")]ref uint aStorage) {
			return aStorage;
		}

		public static char[] GetStorageArray(string aThis, [FieldAccess(Name = "$$Storage$$")]ref char[] aStorage) {
			return aStorage;
		}

		public static void FakeMethod() {
			//char[] xThis = null;
			//CustomImplementation.System.StringImpl.Ctor(null, ref xThis, null);
		}
	}
}
