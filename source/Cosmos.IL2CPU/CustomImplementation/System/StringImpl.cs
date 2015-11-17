using System;
using System.Globalization;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
    [Plug(Target = typeof(string))]
    public static class StringImpl
    {
        public static string Format(IFormatProvider aFormatProvider, string aFormat, object[] aArgs)
        {
            string[] xStrings = new string[1 + 2 + (aArgs.Length * 7) - 1];
            xStrings[0] = aFormat;
            xStrings[1] = "(";
            for (int i = 0; i < aArgs.Length; i++)
            {
                xStrings[2 + (i * 7)] = "Param";
                xStrings[3 + (i * 7)] = i.ToString();
                xStrings[4 + (i * 7)] = "=";
                xStrings[5 + (i * 7)] = "\"";
                xStrings[6 + (i * 7)] = aArgs[i].ToString();
                xStrings[7 + (i * 7)] = "\"";
                if (i < (aArgs.Length - 1))
                {
                    xStrings[8 + (i * 7)] = ",";
                }
            }
            xStrings[xStrings.Length - 1] = ")";
            return String.Concat(xStrings);
        }

        public static bool StartsWith(string aThis, string aSubstring, StringComparison aComparison)
        {
            Console.WriteLine("String.StartsWith not working!");
            throw new NotImplementedException();
        }

        //String concatenation plugs
        public static string Concat(string str0, string str1, string str2)
        {
            return Concat(new string[] { str0, str1, str2 });
        }

        public static string Concat(string str0, string str1)
        {
            return Concat(new string[] { str0, str1 });
        }

        public static string Concat(string str0, string str1, string str2, string str3)
        {
            return Concat(new string[] { str0, str1, str2, str3 });
        }

        //Object concatenation plugs
        public static string Concat(object obj0)
        {
            return obj0.ToString();
        }

        public static string Concat(object obj0, object obj1)
        {
            return Concat(obj0.ToString(), obj1.ToString());
        }

        public static string Concat(object obj0, object obj1, object obj2)
        {
            return Concat(obj0.ToString(), obj1.ToString(), obj2.ToString());
        }

        public static string Concat(object obj0, object obj1, object obj2, object obj3)
        {
            return Concat(new object[] { obj0, obj1, obj2, obj3 });
        }

        //Array concatenation plugs
        public static string Concat(params string[] values)
        {
            int len = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var xValue = values[i];
                if (xValue != null)
                {
                    len += values[i].Length;
                }
            }
            return ConcatArray(values, len);
        }

        public static string Concat(params object[] args)
        {
            string[] values = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var xArg = args[i];
                if (xArg != null)
                {
                    var xStrArg = xArg as string;
                    if (xStrArg != null)
                    {
                        values[i] = xStrArg;
                    }
                    else
                    {
                        values[i] = xArg.ToString();
                    }
                }
            }
            return Concat(values);
        }

        public static string ConcatArray(string[] values, int totalLength)
        {
            char[] xResultChars = new char[totalLength];
            int xCurPos = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var xStr = values[i];
                if (xStr != null)
                {
                    for (int j = 0; j < xStr.Length; j++)
                    {
                        xResultChars[xCurPos] = xStr[j];
                        xCurPos++;
                    }
                }
            }
            var xResult = new String(xResultChars);
            return xResult;
        }

        public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded)
        {
            //Console.Write("PadHelper, totalWidth = ");
            //WriteNumber((uint)totalWidth, 32);
            //Console.WriteLine("");
            char[] cs = new char[totalWidth];

            int pos = aThis.Length;

            if (isRightPadded)
            {
                for (int i = 0; i < aThis.Length; i++) cs[i] = aThis[i];

                for (int i = aThis.Length; i < totalWidth; i++) cs[i] = paddingChar;
            }
            else
            {
                int offset = totalWidth - aThis.Length;
                for (int i = 0; i < aThis.Length; i++) cs[i + offset] = aThis[i];

                for (int i = 0; i < offset; i++) cs[i] = paddingChar;
            }

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos)
        {
            char[] cs = new char[aThis.Length - startpos];
            int j = 0;
            for (int i = startpos; i < aThis.Length; i++) cs[j++] = aThis[i];

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos, int length)
        {
            if (startpos + length > aThis.Length) length = aThis.Length - startpos;

            char[] cs = new char[length];

            int j = 0;
            for (int i = startpos; i < startpos + length; i++) cs[j++] = aThis[i];

            return new string(cs);
        }

        public static string Replace(string aThis, char oldValue, char newValue)
        {
            char[] cs = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] != oldValue) cs[i] = aThis[i];
                else cs[i] = newValue;
            }

            return new string(cs);
        }

        // HACK: We need to redo this once char support is complete (only returns 0, -1).
        public static int CompareTo(string aThis, string other)
        {
            if (aThis.Length != other.Length) return -1;
            for (int i = 0; i < aThis.Length; i++) if (aThis[i] != other[i]) return -1;
            return 0;
        }

        public static int IndexOf(string aThis, char value, int startIndex, int count)
        {
            var xEndIndex = aThis.Length;
            if ((startIndex + count) < xEndIndex)
            {
                xEndIndex = (startIndex + count);
            }
            for (int i = startIndex; i < xEndIndex; i++)
            {
                if (aThis[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        // HACK: TODO - improve efficiency of this.
        //How do we access the raw memory to copy it into a char array?
        public static char[] ToCharArray(string aThis)
        {
            char[] result = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++)
            {
                result[i] = aThis[i];
            }

            return result;
        }

        [PlugMethod(Enabled = false)]
        public static uint GetStorage(string aString)
        {
            return 0;
        }

        //[PlugMethod(Enabled = false)]
        //public static char[] GetStorageArray(string aString) {
        //  return null;
        //}

        private static int[] BuildBadCharTable(char[] needle)
        {
            int[] badShift = new int[256];
            for (int i = 0; i < 256; i++)
            {
                badShift[i] = needle.Length;
            }
            int last = needle.Length - 1;
            for (int i = 0; i < last; i++)
            {
                badShift[(int)needle[i]] = last - i;
            }
            return badShift;
        }

        private static int boyerMooreHorsepool(String pattern, String text)
        {
            char[] needle = pattern.ToCharArray();
            char[] haystack = text.ToCharArray();

            if (needle.Length > haystack.Length)
            {
                return -1;
            }
            int[] badShift = BuildBadCharTable(needle);
            int offset = 0;
            int scan = 0;
            int last = needle.Length - 1;
            int maxoffset = haystack.Length - needle.Length;
            while (offset <= maxoffset)
            {
                for (scan = last; (needle[scan] == haystack[scan + offset]); scan--)
                {
                    if (scan == 0)
                    {
                        //Match found
                        return offset;
                    }
                }
                offset += badShift[(int)haystack[offset + last]];
            }
            return -1;
        }

        public static int IndexOf(string aThis, string aSubstring, int aIdx, int aLength, StringComparison aComparison)
        {
            Console.WriteLine("Be aware: IndexOf(..., StringComparison) not fully supported yet!");
            return boyerMooreHorsepool(aSubstring, aThis.Substring(aIdx, aLength));
        }

        //private static void WriteNumber(uint aValue,
        //                  byte aBitCount)
        //{
        //    uint xValue = aValue;
        //    byte xCurrentBits = aBitCount;
        //    Console.Write("0x");
        //    while (xCurrentBits >= 4)
        //    {
        //        xCurrentBits -= 4;
        //        byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
        //        string xDigitString = null;
        //        switch (xCurrentDigit)
        //        {
        //            case 0:
        //                xDigitString = "0";
        //                goto default;
        //            case 1:
        //                xDigitString = "1";
        //                goto default;
        //            case 2:
        //                xDigitString = "2";
        //                goto default;
        //            case 3:
        //                xDigitString = "3";
        //                goto default;
        //            case 4:
        //                xDigitString = "4";
        //                goto default;
        //            case 5:
        //                xDigitString = "5";
        //                goto default;
        //            case 6:
        //                xDigitString = "6";
        //                goto default;
        //            case 7:
        //                xDigitString = "7";
        //                goto default;
        //            case 8:
        //                xDigitString = "8";
        //                goto default;
        //            case 9:
        //                xDigitString = "9";
        //                goto default;
        //            case 10:
        //                xDigitString = "A";
        //                goto default;
        //            case 11:
        //                xDigitString = "B";
        //                goto default;
        //            case 12:
        //                xDigitString = "C";
        //                goto default;
        //            case 13:
        //                xDigitString = "D";
        //                goto default;
        //            case 14:
        //                xDigitString = "E";
        //                goto default;
        //            case 15:
        //                xDigitString = "F";
        //                goto default;
        //            default:
        //                Console.Write(xDigitString);
        //                break;
        //        }
        //    }
        //}

        public static bool Contains(string aThis, string value)
        {
            if (aThis.IndexOf(value) != -1)
            {
                return true;
            }

            return false;
        }

        public static bool EndsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture)
        {
            return EndsWith(aThis, aSubStr, StringComparison.CurrentCulture);
        }

        public static bool EndsWith(string aThis, string aSubStr, StringComparison aComparison)
        {
            if (aSubStr == null) throw new ArgumentNullException("aSubStr");

            if (aThis == aSubStr) return true;

            if (aSubStr.Length == 0) return true;

            int xLastIdx = aThis.Length - aSubStr.Length;
            for (int i = 0; i < aSubStr.Length; i++)
            {
                if (aThis[xLastIdx + i] != aSubStr[i])
                {
                    return false;
                }
            }
            return true;
        }

        //        System.Int32  System.String.IndexOf(System.String, System.Int32, System.Int32, System.StringComparison)

        public static bool Equals(string aThis, string aThat, StringComparison aComparison)
        {
#warning TODO: implement
            if (aComparison == StringComparison.OrdinalIgnoreCase)
            {
                var xLowerThis = aThis.ToLower();
                var xLowerThat = aThat.ToLower();
                return EqualsHelper(xLowerThis, xLowerThat);
            }
            return EqualsHelper(aThis, aThat);
        }

        public static bool EqualsHelper(string aStrA, string aStrB)
        {
            return aStrA.CompareTo(aStrB) == 0;
        }

        private static bool CharArrayContainsChar(char[] aArray, char aChar)
        {
            for (int i = 0; i < aArray.Length; i++)
            {
                if (aArray[i] == aChar)
                {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOf(string aThis, string aValue)
        {
            return aThis.IndexOf(aValue, 0, aThis.Length, StringComparison.CurrentCulture);
        }

        public static int IndexOfAny(string aThis, char[] aSeparators, int aStartIndex, int aLength)
        {

            if (aSeparators == null)
            {
                throw new ArgumentNullException("aSeparators");
            }

            int xResult = -1;
            for (int i = 0; i < aSeparators.Length; i++)
            {
                var xValue = IndexOf(aThis, aSeparators[i], aStartIndex, aLength);
                if (xValue < xResult || xResult == -1)
                {
                    xResult = xValue;
                }
            }
            return xResult;
        }

        public static string Insert(string aThis, int aStartPos, string aValue)
        {
            return aThis.Substring(0, aStartPos) + aValue + aThis.Substring(aStartPos);
        }

        public static int LastIndexOf(string aThis, char aChar, int aStartIndex, int aCount)
        {
            return LastIndexOfAny(aThis, new char[] { aChar }, aStartIndex, aCount);
        }

        public static int LastIndexOfAny(string aThis, char[] aChars, int aStartIndex, int aCount)
        {
            for (int i = 0; i < aCount; i++)
            {
                if (CharArrayContainsChar(aChars, aThis[aStartIndex - i]))
                {
                    return aStartIndex - i;
                }
            }
            return -1;
        }

        public static int nativeCompareOrdinalEx(string aStrA, int aIndexA, string aStrB, int aIndexB, int aCount)
        {
            //Just a basic implementation
            if (aStrA == aStrB) return 0;
            else return -1;
        }

        public static bool StartsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture)
        {
            for (int i = 0; i < aSubStr.Length; i++)
            {
                if (aThis[i] != aSubStr[i]) return false;
            }
            return true;
        }

        public static string Remove(string aThis, int aStart, int aCount)
        {
            return aThis.Substring(0, aStart) + aThis.Substring(aStart + aCount, aThis.Length - (aStart + aCount));
        }

        public static string Replace(string aThis, string oldValue, string newValue)
        {
            while (aThis.IndexOf(oldValue) != -1)
            {
                int xIndex = aThis.IndexOf(oldValue);
                aThis = aThis.Remove(xIndex, oldValue.Length);
                aThis = aThis.Insert(xIndex, newValue);
            }
            return aThis;
        }

        public static string ToLower(string aThis, CultureInfo aCulture)
        {
            return ChangeCasing(aThis, 65, 90, 32);
        }

        public static string ToUpper(string aThis, CultureInfo aCulture)
        {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        public static string ToUpper(string aThis)
        {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        private static string ChangeCasing(string aValue, int lowerAscii, int upperAscii, int offset)
        {
            char[] xChars = new char[aValue.Length];

            for (int i = 0; i < aValue.Length; i++)
            {
                int xAsciiCode = (int)aValue[i];
                if ((xAsciiCode <= upperAscii) && (xAsciiCode >= lowerAscii)) xChars[i] = (char)(xAsciiCode + offset);
                else xChars[i] = aValue[i];
            }

            return new string(xChars);
        }

        public static string ToString(string aThis)
        {
            if (aThis == null)
            {
                return string.Empty;
            }

            return aThis;
        }

        /*public int IndexOf(char c)
       {
           // TODO: We can't get 'this'
           //string me = ToString();
           //for (int i = 0; i < me.Length; i++)
           //{
           //    if (me[i] == c)
           //        return i;
           //}
           return -1;
       }*/
    }
}