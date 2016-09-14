using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(char))]
    public static class CharImpl
    {
        public static void Cctor()
        {
        }

        public static bool IsDigit(char aChar)
        {
            return (aChar >= '0' && aChar <= '9');
        }

        public static bool IsDigit(string aString, int aIndex)
        {
            if (aString == null)
            {
                throw new ArgumentNullException("aString");
            }

            if (((uint)aIndex) >= ((uint)aString.Length))
            {
                throw new ArgumentOutOfRangeException("aIndex");
            }

            char c = aString[aIndex];
            return (c >= '0' && c <= '9');
        }

        public static string ToString(ref char aThis)
        {
            char[] xResult = new char[1];
            xResult[0] = aThis;
            return new string(xResult);
        }

        public static bool Equals(ref char aThis, char that)
        {
            return aThis == that;
        }

        public static char ToUpper(char aThis)
        {
            // todo: properly implement Char.ToUpper()
            return aThis;
        }

        public static bool IsWhiteSpace(char aChar)
        {
            return aChar == ' ' || aChar == '\t';
        }
    }
}
