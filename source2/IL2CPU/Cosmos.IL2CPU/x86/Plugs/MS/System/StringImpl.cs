using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.MS.System {
	[Plug(Target = typeof(String), IsMicrosoftdotNETOnly = true)]
	public static class StringImpl {

        public static unsafe void Ctor(string aThis, char[] aChars,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            aStringLength = aChars.Length;
            for (int i = 0; i < aChars.Length; i++) {
                aFirstChar[i] = aChars[i];
            }
        }

        public static unsafe void Ctor(string aThis, char[] aChars, int start, int length,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            aStringLength = length;
			for (int i = 0; i < length; i++) {
                aFirstChar[i] = aChars[start+i];
            }
        }

		public static unsafe void Ctor(String aThis, char aChar, int aLength,
			[FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
			[FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
		{
			aStringLength = aLength;
			for (int i = 0; i < aLength; i++)
			{
				aFirstChar[i] = aChar;
			}
		}

        [PlugMethod(Signature = "System_Int32__System_String_get_Length__")]
		public static unsafe int get_Length(int* aThis, [FieldAccess(Name = "System.Int32 System.String.m_stringLength")]ref int aLength) {
			return aLength;
		}

		public static unsafe char get_Chars(byte* aThis, int aIndex) {
            var xCharIdx = (char*)(aThis + 16);
            return xCharIdx[aIndex];
		}

        public static string FastAllocateString(int length) {
			return new String(new char[length]);
		}
	}
}