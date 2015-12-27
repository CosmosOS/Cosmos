using System;
using System.Globalization;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System
{
    [Plug(Target = typeof(string), IsMicrosoftdotNETOnly = true)]
    public static class StringImpl
    {
        public static unsafe void Ctor(
            string aThis,
            char[] aChars,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            aStringEmpty = "";
            aStringLength = aChars.Length;
            for (int i = 0; i < aChars.Length; i++)
            {
                aFirstChar[i] = aChars[i];
            }
        }

        public static unsafe void Ctor(
            string aThis,
            char[] aChars,
            int start,
            int length,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            aStringEmpty = "";
            aStringLength = length;
            for (int i = 0; i < length; i++)
            {
                aFirstChar[i] = aChars[start + i];
            }
        }

        public static unsafe void Ctor(
            string aThis,
            char aChar,
            int aLength,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            aStringEmpty = "";
            aStringLength = aLength;
            for (int i = 0; i < aLength; i++)
            {
                aFirstChar[i] = aChar;
            }
        }

        public static unsafe int get_Length(
            int* aThis,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aLength)
        {
            return aLength;
        }

        public static unsafe char get_Chars(uint* aThis, int aIndex)
        {
            // todo: change to use a FieldAccessAttribute, to get the pointer to the first character and go from there

            // we first need to dereference the handle to a pointer.
            uint xActualThis = aThis[0];
            var xCharIdx = (char*)(xActualThis + 16);
            return xCharIdx[aIndex];
        }

        public static string Format(string aFormat, object aArg1)
        {
            if (aArg1 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }
            var xO = new object[1];
            xO[0] = aArg1;
            return FormatHelper(null, aFormat, xO);
        }

        public static string Format(string aFormat, object aArg1, object aArg2)
        {
            if ((aArg1 == null) || (aArg2 == null))
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }
            var xO = new object[2];
            xO[0] = aArg1;
            xO[1] = aArg2;
            return FormatHelper(null, aFormat, xO);
        }

        public static string Format(string aFormat, object aArg1, object aArg2, object aArg3)
        {
            if ((aArg1 == null) || (aArg2 == null) || (aArg3 == null))
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }
            var xO = new object[3];
            xO[0] = aArg1;
            xO[1] = aArg2;
            xO[2] = aArg3;
            return FormatHelper(null, aFormat, xO);
        }

        public static string Format(string aFormat, params object[] aArgs)
        {
            if (aArgs == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }
            return FormatHelper(null, aFormat, aArgs);
        }

        public static string Format(IFormatProvider aFormatProvider, string aFormat, params object[] aArgs)
        {
            if (aArgs == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }
            return FormatHelper(aFormatProvider, aFormat, aArgs);
        }

        public static string FormatHelper(IFormatProvider aFormatProvider, string aFormat, object[] aArgs)
        {
            char[] xCharArray = aFormat.ToCharArray();
            string xFormattedString = string.Empty;
            bool xFoundPlaceholder = false;
            string xParamNumber = string.Empty;
            bool xParamNumberDone = true;
            for (int i = 0; i < xCharArray.Length; i++)
            {
                if (xFoundPlaceholder)
                {
                    if (xCharArray[i] == '{')
                    {
                        throw new FormatException("The format string provided is invalid.");
                    }
                    if (xCharArray[i] == '}')
                    {
                        int xParamIndex = StringHelper.GetStringToNumber(xParamNumber);
                        if ((xParamIndex < aArgs.Length - 1) && (aArgs[xParamIndex] != null))
                        {
                            string xParamValue = aArgs[xParamIndex].ToString();
                            xFormattedString = string.Concat(xFormattedString, xParamValue);
                        }
                        xFoundPlaceholder = false;
                        xParamNumberDone = true;
                        xParamNumber = string.Empty;
                    }
                    else if (xCharArray[i] == ':')
                    {
                        xParamNumberDone = true;
                        // TODO: Need to handle different formats. (X, N, etc)
                    }
                    else if ((char.IsDigit(xCharArray[i])) && (!xParamNumberDone))
                    {
                        xParamNumber = string.Concat(xParamNumber, xCharArray[i]);
                    }
                }
                else if (xCharArray[i] == '{')
                {
                    xFoundPlaceholder = true;
                    xParamNumberDone = false;
                    xParamNumber = string.Empty;
                }
                else
                {
                    xFormattedString = string.Concat(xFormattedString, xCharArray[i]);
                }
            }

            return xFormattedString;
        }

        public static bool StartsWith(string aThis, string aSubstring, StringComparison aComparison)
        {
            Console.WriteLine("String.StartsWith not working!");
            throw new NotImplementedException();
        }

        //String concatenation plugs
        public static string Concat(string str0)
        {
            return str0;
        }

        public static string Concat(string str0, string str1)
        {
            return Concat(new[] { str0, str1 });
        }

        public static string Concat(string str0, string str1, string str2)
        {
            return Concat(new[] { str0, str1, str2 });
        }

        public static string Concat(string str0, string str1, string str2, string str3)
        {
            return Concat(new[] { str0, str1, str2, str3 });
        }

        //Object concatenation plugs
        public static string Concat(object obj0)
        {
            return obj0?.ToString();
        }

        public static string Concat(object obj0, object obj1)
        {
            return Concat(obj0?.ToString(), obj1?.ToString());
        }

        public static string Concat(object obj0, object obj1, object obj2)
        {
            return Concat(obj0?.ToString(), obj1?.ToString(), obj2?.ToString());
        }

        public static string Concat(object obj0, object obj1, object obj2, object obj3)
        {
            return Concat(new[] { obj0?.ToString(), obj1?.ToString(), obj2?.ToString(), obj3?.ToString() });
        }

        //Array concatenation plugs
        public static string Concat(params string[] values)
        {
            if (values != null)
            {
                int len = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    string xValue = values[i];
                    if (xValue != null)
                    {
                        len += values[i].Length;
                    }
                }
                return ConcatArray(values, len);
            }
            return string.Empty;
        }

        public static string Concat(params object[] args)
        {
            if (args != null)
            {
                var values = new string[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    var xArg = args[i];
                    if (xArg != null)
                    {
                        string xStrArg = xArg as string;
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
            return string.Empty;
        }

        public static string ConcatArray(string[] values, int totalLength)
        {
            if (values != null)
            {
                var xResultChars = new char[totalLength];
                int xCurPos = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    string xStr = values[i];
                    if (xStr != null)
                    {
                        for (int j = 0; j < xStr.Length; j++)
                        {
                            xResultChars[xCurPos] = xStr[j];
                            xCurPos++;
                        }
                    }
                }
                string xResult = new string(xResultChars);
                return xResult;
            }
            return string.Empty;
        }

        public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded)
        {
            //Console.Write("PadHelper, totalWidth = ");
            //WriteNumber((uint)totalWidth, 32);
            //Console.WriteLine("");
            var cs = new char[totalWidth];

            int pos = aThis.Length;

            if (isRightPadded)
            {
                for (int i = 0; i < aThis.Length; i++)
                {
                    cs[i] = aThis[i];
                }

                for (int i = aThis.Length; i < totalWidth; i++)
                {
                    cs[i] = paddingChar;
                }
            }
            else
            {
                int offset = totalWidth - aThis.Length;
                for (int i = 0; i < aThis.Length; i++)
                {
                    cs[i + offset] = aThis[i];
                }

                for (int i = 0; i < offset; i++)
                {
                    cs[i] = paddingChar;
                }
            }

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos)
        {
            var cs = new char[aThis.Length - startpos];
            int j = 0;
            for (int i = startpos; i < aThis.Length; i++)
            {
                cs[j++] = aThis[i];
            }

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos, int length)
        {
            if (startpos + length > aThis.Length)
            {
                length = aThis.Length - startpos;
            }

            var cs = new char[length];

            int j = 0;
            for (int i = startpos; i < startpos + length; i++)
            {
                cs[j++] = aThis[i];
            }

            return new string(cs);
        }

        public static string Replace(string aThis, char oldValue, char newValue)
        {
            var cs = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] != oldValue)
                {
                    cs[i] = aThis[i];
                }
                else
                {
                    cs[i] = newValue;
                }
            }

            return new string(cs);
        }

        // HACK: We need to redo this once char support is complete (only returns 0, -1).
        public static int CompareTo(string aThis, string other)
        {
            if (aThis.Length != other.Length)
            {
                return -1;
            }
            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] != other[i])
                {
                    return -1;
                }
            }
            return 0;
        }

        public static int IndexOf(string aThis, char value, int startIndex, int count)
        {
            int xEndIndex = aThis.Length;
            if (startIndex + count < xEndIndex)
            {
                xEndIndex = startIndex + count;
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
            var result = new char[aThis.Length];

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
            var badShift = new int[256];
            for (int i = 0; i < 256; i++)
            {
                badShift[i] = needle.Length;
            }
            int last = needle.Length - 1;
            for (int i = 0; i < last; i++)
            {
                badShift[needle[i]] = last - i;
            }
            return badShift;
        }

        private static int boyerMooreHorsepool(string pattern, string text)
        {
            var needle = pattern.ToCharArray();
            var haystack = text.ToCharArray();

            if (needle.Length > haystack.Length)
            {
                return -1;
            }
            var badShift = BuildBadCharTable(needle);
            int offset = 0;
            int scan = 0;
            int last = needle.Length - 1;
            int maxoffset = haystack.Length - needle.Length;
            while (offset <= maxoffset)
            {
                for (scan = last; needle[scan] == haystack[scan + offset]; scan--)
                {
                    if (scan == 0)
                    {
                        //Match found
                        return offset;
                    }
                }
                offset += badShift[haystack[offset + last]];
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
            if (aSubStr == null)
            {
                throw new ArgumentNullException("aSubStr");
            }

            if (aThis == aSubStr)
            {
                return true;
            }

            if (aSubStr.Length == 0)
            {
                return true;
            }

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
                string xLowerThis = aThis.ToLower();
                string xLowerThat = aThat.ToLower();
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
                int xValue = IndexOf(aThis, aSeparators[i], aStartIndex, aLength);
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
            return LastIndexOfAny(aThis, new[] { aChar }, aStartIndex, aCount);
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
            if (aStrA == aStrB)
            {
                return 0;
            }
            return -1;
        }

        public static bool StartsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture)
        {
            for (int i = 0; i < aSubStr.Length; i++)
            {
                if (aThis[i] != aSubStr[i])
                {
                    return false;
                }
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
            var xChars = new char[aValue.Length];

            for (int i = 0; i < aValue.Length; i++)
            {
                int xAsciiCode = aValue[i];
                if ((xAsciiCode <= upperAscii) && (xAsciiCode >= lowerAscii))
                {
                    xChars[i] = (char)(xAsciiCode + offset);
                }
                else
                {
                    xChars[i] = aValue[i];
                }
            }

            return new string(xChars);
        }

        public static string ToString(string aThis)
        {
            return aThis;
        }

        public static string FastAllocateString(int length)
        {
            return new string(new char[length]);
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
