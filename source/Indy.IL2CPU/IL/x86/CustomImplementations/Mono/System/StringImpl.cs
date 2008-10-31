using System;
using System.Linq;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.Mono.System {
	[Plug(Target = typeof(String), IsMonoOnly = true)]
	[PlugField(FieldId = "$$Storage$$", FieldType = typeof(char[]))]
	[PlugField(FieldId = "System.Char System.String.start_char", IsExternalValue = true)]
	public static class StringImpl {
		[PlugMethod(Signature = "System_Void__System_String__ctor_System_Char____System_Int32__System_Int32_")]
		public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")]ref Char[] aStorage, Char[] aChars, int aStartIndex, int aLength,
			[FieldAccess(Name = "System.Int32 System.String.length")] ref int aStrLength,
			[FieldAccess(Name = "System.Char System.String.start_char")] ref char* aFirstChar) {
			Char[] newChars = new Char[aLength];
			Array.Copy(aChars, aStartIndex, newChars, 0, aLength);
			aStorage = newChars;
			aStrLength = newChars.Length;
			fixed (char* xFirstChar = &aStorage[0]) {
				aFirstChar = xFirstChar;
			}
		}

		[PlugMethod(Signature = "System_Void__System_String__ctor_System_Char___")]
		public static unsafe void Ctor(String aThis, [FieldAccess(Name = "$$Storage$$")] ref Char[] aStorage, Char[] aChars,
			[FieldAccess(Name = "System.Int32 System.String.length")]ref int aStringLength,
			[FieldAccess(Name = "System.Char System.String.start_char")] ref char* aFirstChar) {
			aStorage = aChars;
			aStringLength = aChars.Length;
			fixed(char* xFirstChar = &aStorage[0]){
				aFirstChar = xFirstChar;
			}
		}

		public static unsafe int get_Length(int* aThis, [FieldAccess(Name = "System.Int32 System.String.length")]ref int aLength) {
			return aLength;
		}

		public static unsafe char get_Chars(uint* aThis, [FieldAccess(Name = "System.Char System.String.start_char")]ref char* aStorage, int aIndex) {
			return aStorage[aIndex];
		}

		public static string FastAllocateString(int length) {
			return new String(new char[length]);
		}

		//public static unsafe string GetStringForStringBuilder(string value, int startIndex, int length, int capacity) {
		//    string str = FastAllocateString(capacity);
		//    if (value.Length == 0x0) {
		//        str.SetLength(0x0);
		//        return str;
		//    }
		//    fixed (char* chRef = &str.m_firstChar) {
		//        fixed (char* chRef2 = &value.m_firstChar) {
		//            wstrcpy(chRef, chRef2 + startIndex, length);
		//        }
		//    }
		//    str.SetLength(length);
		//    return str;
		//    return null;
		//}

		public static unsafe void wstrcpy(char* dmem, char* smem, int charCount) {
			for (int i = 0; i < charCount; i++) {
				dmem[i] = smem[i];
			}
		}



		public static void cctor() {
		}
	}
}
