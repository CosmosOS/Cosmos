using System;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.X86.Plugs.CustomImplementations.MS.System {
	[Plug(Target = typeof(String), IsMicrosoftdotNETOnly = true)]
	public static class StringImpl {

        public static unsafe void Ctor(string aThis, char[] aChars,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            Debugger.DoSend("In Ctor(char[])");
            Debugger.DoSend("aChars.Length");
            Debugger.DoSendNumber((uint)aChars.Length);
            aStringLength = aChars.Length;
            Debugger.DoSend("aStringLength");
            Debugger.DoSendNumber((uint)aStringLength);
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

        public static unsafe int get_Length(int* aThis, [FieldAccess(Name = "System.Int32 System.String.m_stringLength")]ref int aLength)
        {
            return aLength;
		}

		public static unsafe char get_Chars(uint* aThis, int aIndex)
		{
            // todo: change to use a FieldAccessAttribute, to get the pointer to the first character and go from there

            // we first need to dereference the handle to a pointer.
            var xActualThis = (uint)aThis[0];
            var xCharIdx = (char*)(xActualThis + 16);
            return xCharIdx[aIndex];
		}

        public static string FastAllocateString(int length) {
			return new String(new char[length]);
		}
	}
}
