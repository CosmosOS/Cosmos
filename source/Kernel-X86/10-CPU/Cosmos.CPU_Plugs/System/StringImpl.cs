//#define COSMOSDEBUG
using System;
using System.Globalization;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.CPU_Plugs.System {
    [Plug(Target = typeof(string))]
    public static class StringImpl {
        internal static Debugger mDebugger = new Debugger("CPU", "String Plugs");

        public static unsafe void Ctor(
            string aThis,
            char* aChars,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            mDebugger.SendInternal("String.Ctor(string, char*)");

            aStringEmpty = "";
            while (*aChars != '\0') {
                aChars++;
                aStringLength++;
            }
            for (int i = 0; i < aStringLength; i++) {
                aFirstChar[i] = aChars[i];
            }
        }

        public static unsafe void Ctor(
            string aThis,
            char* aChars,
            int start,
            int length,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            aStringEmpty = "";
            aStringLength = length;
            for (int i = 0; i < length; i++) {
                aFirstChar[i] = aChars[start + i];
            }
        }

        public static unsafe void Ctor(
            string aThis,
            char[] aChars,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            aStringEmpty = "";
            aStringLength = aChars.Length;
            for (int i = 0; i < aChars.Length; i++) {
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
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            aStringEmpty = "";
            aStringLength = length;
            for (int i = 0; i < length; i++) {
                aFirstChar[i] = aChars[start + i];
            }
        }


        public static unsafe void Ctor(
            string aThis,
            char aChar,
            int aLength,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            aStringEmpty = "";
            aStringLength = aLength;
            for (int i = 0; i < aLength; i++) {
                aFirstChar[i] = aChar;
            }
        }


        public static unsafe int get_Length(
            [ObjectPointerAccess] uint* aThis,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aLength) {
            return aLength;
        }

        public static unsafe char get_Chars(
            [ObjectPointerAccess] uint* aThis,
            int aIndex,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar) {
            return *(aFirstChar + aIndex);
        }

        public static bool IsAscii(string aThis) {
            for (int i = 0; i < aThis.Length; i++) {
                if (aThis[i] >= 0x80) {
                    return false;
                }
            }
            return true;
        }

        public static string Format(string aFormat, object aArg0) {
            if (aArg0 == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArg0);
        }

        public static string Format(string aFormat, object aArg0, object aArg1) {
            if (aFormat == null) {
                throw new ArgumentNullException(nameof(aFormat));
            }
            if (aArg0 == null) {
                throw new ArgumentNullException(nameof(aArg0));
            }
            if (aArg1 == null) {
                throw new ArgumentNullException(nameof(aArg1));
            }

            return FormatHelper(null, aFormat, aArg0, aArg1);
        }

        public static string Format(string aFormat, object aArg0, object aArg1, object aArg2) {
            if (aArg0 == null || aArg1 == null || aArg2 == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArg0, aArg1, aArg2);
        }

        public static string Format(string aFormat, params object[] aArgs) {
            if (aArgs == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArgs);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0) {
            if (aArg0 == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0, object aArg1) {
            if (aArg0 == null | aArg1 == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0, aArg1);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0, object aArg1, object aArg2) {
            if (aArg0 == null | aArg1 == null || aArg2 == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0, aArg1, aArg2);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, params object[] aArgs) {
            if (aArgs == null) {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArgs);
        }

        public static int GetStringToNumber(string aString) {
            bool xIsNegative = false;
            int xNumber = 0;
            if (!string.IsNullOrWhiteSpace(aString)) {
                char[] xCharArray = aString.ToCharArray();
                for (int i = 0; i < xCharArray.Length; i++) {
                    if (char.IsDigit(xCharArray[i])) {
                        int xValue = xCharArray[i] - '0';
                        int xMax = xCharArray.Length - 1;
                        for (int j = 0; j < xMax - i; i++) {
                            xValue *= 10;
                        }

                        xNumber += xValue;
                    } else if (xCharArray[i] == '-') {
                        xIsNegative = true;
                    } else if (xCharArray[i] == '.') {
                        break;
                    } else {
                        throw new FormatException("The string parameter is not a number and is invalid.");
                    }
                }
            }

            if (xIsNegative) {
                xNumber *= -1;
            }

            return xNumber;
        }

        internal static string FormatHelper(IFormatProvider aFormatProvider, string aFormat, params object[] aArgs) {
            char[] xCharArray = aFormat.ToCharArray();
            string xFormattedString = string.Empty, xStaticString;
            bool xFoundPlaceholder = false, xParamNumberDone = true;
            int xStartParamNumber = -1, xEndParamNumber = -1, xLastPlaceHolder = 0;

            for (int i = 0; i < xCharArray.Length; i++) {
                if (xFoundPlaceholder) {
                    if (xCharArray[i] == '{') {
                        throw new FormatException("The format string provided is invalid.");
                    }
                    if (xCharArray[i] == '}') {
                        mDebugger.SendInternal("Found closing placeholder.");
                        if (xEndParamNumber < 0) {
                            xEndParamNumber = i;
                        }
                        string xParamNumber = aFormat.Substring(xStartParamNumber, xEndParamNumber - xStartParamNumber);
                        mDebugger.SendInternal("Calling StringHelper.GetStringToNumber");
                        mDebugger.SendInternal(xParamNumber);
                        int xParamIndex = GetStringToNumber(xParamNumber);
                        mDebugger.SendInternal("Converted paramindex to a number.");
                        if ((xParamIndex < aArgs.Length) && (aArgs[xParamIndex] != null)) {
                            string xParamValue = aArgs[xParamIndex].ToString();
                            xFormattedString = string.Concat(xFormattedString, xParamValue);
                            mDebugger.SendInternal("xParamValue =");
                            mDebugger.SendInternal(xParamValue);
                            mDebugger.SendInternal("xFormattedString =");
                            mDebugger.SendInternal(xFormattedString);

                        }
                        xFoundPlaceholder = false;
                        xParamNumberDone = true;
                        xStartParamNumber = -1;
                        xEndParamNumber = -1;
                        xLastPlaceHolder = i + 1;
                    } else if (xCharArray[i] == ':') {
                        xParamNumberDone = true;
                        xEndParamNumber = i;
                        // TODO: Need to handle different formats. (X, N, etc)
                    } else if ((char.IsDigit(xCharArray[i])) && (!xParamNumberDone)) {
                        mDebugger.SendInternal("Getting param number.");
                        if (xStartParamNumber < 0) {
                            xStartParamNumber = i;
                        }
                    }
                } else if (xCharArray[i] == '{') {
                    mDebugger.SendInternal("Found opening placeholder");
                    xStaticString = aFormat.Substring(xLastPlaceHolder, i - xLastPlaceHolder);
                    xFormattedString = string.Concat(xFormattedString, xStaticString);
                    xFoundPlaceholder = true;
                    xParamNumberDone = false;
                }
            }

            xStaticString = aFormat.Substring(xLastPlaceHolder, aFormat.Length - xLastPlaceHolder);
            xFormattedString = string.Concat(xFormattedString, xStaticString);

            return xFormattedString;
        }

        public static bool StartsWith(string aThis, string aSubstring, StringComparison aComparison) {
            Char[] di = aThis.ToCharArray();
            Char[] ci = aSubstring.ToCharArray();
            if (aSubstring.Length > aThis.Length) {
                return false;
            }
            for (int i = 0; i < ci.Length; i++) {
                if (di[i] != ci[i]) {
                    return false;

                }
            }
            return true;
        }

        public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded) {
            var cs = new char[totalWidth];

            int pos = aThis.Length;

            if (isRightPadded) {
                for (int i = 0; i < aThis.Length; i++) {
                    cs[i] = aThis[i];
                }

                for (int i = aThis.Length; i < totalWidth; i++) {
                    cs[i] = paddingChar;
                }
            } else {
                int offset = totalWidth - aThis.Length;
                for (int i = 0; i < aThis.Length; i++) {
                    cs[i + offset] = aThis[i];
                }

                for (int i = 0; i < offset; i++) {
                    cs[i] = paddingChar;
                }
            }

            return new string(cs);
        }

        public static string Replace(string aThis, char oldValue, char newValue) {
            var cs = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++) {
                if (aThis[i] != oldValue) {
                    cs[i] = aThis[i];
                } else {
                    cs[i] = newValue;
                }
            }

            return new string(cs);
        }

        // HACK: We need to redo this once char support is complete (only returns 0, -1).
        public static int CompareTo(string aThis, string other) {
            if (aThis.Length != other.Length) {
                return -1;
            }
            for (int i = 0; i < aThis.Length; i++) {
                if (aThis[i] != other[i]) {
                    return -1;
                }
            }
            return 0;
        }

        public static int IndexOf(string aThis, char value, int startIndex, int count) {
            int xEndIndex = aThis.Length;
            if (startIndex + count < xEndIndex) {
                xEndIndex = startIndex + count;
            }
            for (int i = startIndex; i < xEndIndex; i++) {
                if (aThis[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        // HACK: TODO - improve efficiency of this.
        //How do we access the raw memory to copy it into a char array?
        public static char[] ToCharArray(string aThis) {
            var result = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++) {
                result[i] = aThis[i];
            }

            return result;
        }

        [PlugMethod(Enabled = false)]
        public static uint GetStorage(string aString) {
            return 0;
        }

        //[PlugMethod(Enabled = false)]
        //public static char[] GetStorageArray(string aString) {
        //  return null;
        //}

        private static int[] BuildBadCharTable(char[] needle) {
            var badShift = new int[256];
            for (int i = 0; i < 256; i++) {
                badShift[i] = needle.Length;
            }
            int last = needle.Length - 1;
            for (int i = 0; i < last; i++) {
                badShift[needle[i]] = last - i;
            }
            return badShift;
        }

        private static int boyerMooreHorsepool(string pattern, string text) {
            var needle = pattern.ToCharArray();
            var haystack = text.ToCharArray();

            if (needle.Length > haystack.Length) {
                return -1;
            }
            var badShift = BuildBadCharTable(needle);
            int offset = 0;
            int scan = 0;
            int last = needle.Length - 1;
            int maxoffset = haystack.Length - needle.Length;
            while (offset <= maxoffset) {
                for (scan = last; needle[scan] == haystack[scan + offset]; scan--) {
                    if (scan == 0) {
                        //Match found
                        return offset;
                    }
                }
                offset += badShift[haystack[offset + last]];
            }
            return -1;
        }

        public static int IndexOf(string aThis, string aSubstring, int aIdx, int aLength, StringComparison aComparison) {
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

        public static bool Contains(string aThis, string value) {
            Char[] di = aThis.ToCharArray();
            Char[] ci = value.ToCharArray();
            if (value.Length == aThis.Length) {
                if (value == aThis) {
                    return true;
                } else {
                    return false;
                }
            } else if (!(value.Length > aThis.Length) && (value.Length != aThis.Length)) {
                for (int i = 0; i < aThis.Length; i++) {
                    if (di[i] == ci[0]) {
                        for (int j = 1; j < value.Length; j++) {
                            if (di[i + j] != ci[j]) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool EndsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture) {
            return EndsWith(aThis, aSubStr, StringComparison.CurrentCulture);
        }

        public static bool EndsWith(string aThis, string aSubStr, StringComparison aComparison) {
            Char[] di = aThis.ToCharArray();
            Char[] ci = aSubStr.ToCharArray();
            if (aThis.Length == aSubStr.Length) {
                if (aThis == aSubStr) {
                    return true;
                }
                return false;
            } else if (aThis.Length < aSubStr.Length) {
                return false;
            } else {
                for (int i = 0; i < ci.Length; i++) {
                    if (di[aThis.Length - aSubStr.Length + i] != ci[i]) {
                        return false;
                    }
                }
                return true;
            }
        }

        //        System.Int32  System.String.IndexOf(System.String, System.Int32, System.Int32, System.StringComparison)

        public static bool Equals(string aThis, string aThat, StringComparison aComparison) {
#warning TODO: implement
            if (aComparison == StringComparison.OrdinalIgnoreCase) {
                string xLowerThis = aThis.ToLower();
                string xLowerThat = aThat.ToLower();
                return EqualsHelper(xLowerThis, xLowerThat);
            }
            return EqualsHelper(aThis, aThat);
        }

        public static bool EqualsHelper(string aStrA, string aStrB) {
            return aStrA.CompareTo(aStrB) == 0;
        }

        private static bool CharArrayContainsChar(char[] aArray, char aChar) {
            for (int i = 0; i < aArray.Length; i++) {
                if (aArray[i] == aChar) {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOf(string aThis, string aValue) {
            return aThis.IndexOf(aValue, 0, aThis.Length, StringComparison.CurrentCulture);
        }

        public static int IndexOfAny(string aThis, char[] aSeparators, int aStartIndex, int aLength) {
            if (aSeparators == null) {
                throw new ArgumentNullException("aSeparators");
            }

            int xResult = -1;
            for (int i = 0; i < aSeparators.Length; i++) {
                int xValue = IndexOf(aThis, aSeparators[i], aStartIndex, aLength);
                if (xValue < xResult || xResult == -1) {
                    xResult = xValue;
                }
            }
            return xResult;
        }

        public static string Insert(string aThis, int aStartPos, string aValue) {
            return aThis.Substring(0, aStartPos) + aValue + aThis.Substring(aStartPos);
        }

        public static int LastIndexOf(string aThis, char aChar, int aStartIndex, int aCount) {
            return LastIndexOfAny(aThis, new[] { aChar }, aStartIndex, aCount);
        }

        public static int LastIndexOfAny(string aThis, char[] aChars, int aStartIndex, int aCount) {
            for (int i = 0; i < aCount; i++) {
                if (CharArrayContainsChar(aChars, aThis[aStartIndex - i])) {
                    return aStartIndex - i;
                }
            }
            return -1;
        }

        public static int nativeCompareOrdinalEx(string aStrA, int aIndexA, string aStrB, int aIndexB, int aCount) {
            //mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrA|aIndexA = {aStrA}|{aIndexA}, aStrB|aIndexB = {aStrB}|{aIndexB}, aCount = {aCount}");
            if (aCount < 0) {
                throw new ArgumentOutOfRangeException("aCount");
            }

            if (aIndexA < 0 || aIndexA > aStrA.Length) {
                throw new ArgumentOutOfRangeException("aIndexA");
            }

            if (aIndexB < 0 || aIndexB > aStrB.Length) {
                throw new ArgumentOutOfRangeException("aIndexB");
            }

            if (aStrA == null) {
                mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA is null");
                if (aStrB == null) {
                    mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is null");
                    mDebugger.SendInternal($"nativeCompareOrdinalEx : returning 0");
                    return 0;
                }
                mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is not null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : returning -1");
                return -1;
            }
            if (aStrB == null) {
                mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA is not null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : returning 1");
                return 1;
            }
            int xLengthA = Math.Min(aStrA.Length, aCount - aIndexA);
            int xLengthB = Math.Min(aStrB.Length, aCount - aIndexB);
            //mDebugger.SendInternal($"nativeCompareOrdinalEx : xLengthA = {xLengthA}");
            //mDebugger.SendInternal($"nativeCompareOrdinalEx : xLengthB = {xLengthB}");

            if (xLengthA == xLengthB && aIndexA == aIndexB && ReferenceEquals(aStrA, aStrB)) {
                mDebugger.SendInternal("nativeCompareOrdinalEx : xLengthA == xLengthB && aIndexA == aIndexB && aStrA is the same object asaStrB, returning 0");
                return 0;
            }

            int xResult = 0;
            if (xLengthA != xLengthB) {
                xResult = xLengthA - xLengthB;
                mDebugger.SendInternal("nativeCompareOrdinalEx : xLengthA != xLengthB, returning " + xResult);
            }

            for (int i = 0; i < xLengthA; i++) {
                if (aStrA != aStrB) {
                    xResult = (byte)aStrA[i] - (byte)aStrB[i];
                    mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA[i] != aStrB[i], returning " + xResult);
                    return xResult;
                }
            }
            return xResult;
        }

        public static bool StartsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture) {
            Char[] di = aThis.ToCharArray();
            Char[] ci = aSubStr.ToCharArray();
            if (aSubStr.Length > aThis.Length) {
                return false;
            }
            for (int i = 0; i < ci.Length; i++) {
                if (di[i] != ci[i]) {
                    return false;

                }
            }
            return true;
        }

        //public static string Remove(string aThis, int aStart, int aCount)
        //{
        //    return aThis.Substring(0, aStart) + aThis.Substring(aStart + aCount, aThis.Length - (aStart + aCount));
        //}

        public static string Replace(string aThis, string oldValue, string newValue) {
            while (aThis.IndexOf(oldValue) != -1) {
                int xIndex = aThis.IndexOf(oldValue);
                aThis = aThis.Remove(xIndex, oldValue.Length);
                aThis = aThis.Insert(xIndex, newValue);
            }
            return aThis;
        }

        public static string ToLower(string aThis, CultureInfo aCulture) {
            return ChangeCasing(aThis, 65, 90, 32);
        }

        public static string ToUpper(string aThis, CultureInfo aCulture) {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        public static string ToUpper(string aThis) {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        private static string ChangeCasing(string aValue, int lowerAscii, int upperAscii, int offset) {
            var xChars = new char[aValue.Length];

            for (int i = 0; i < aValue.Length; i++) {
                int xAsciiCode = aValue[i];
                if ((xAsciiCode <= upperAscii) && (xAsciiCode >= lowerAscii)) {
                    xChars[i] = (char)(xAsciiCode + offset);
                } else {
                    xChars[i] = aValue[i];
                }
            }

            return new string(xChars);
        }

        public static string FastAllocateString(int aLength) {
            return new string(new char[aLength]);
        }

        [PlugMethod(IsOptional = true)]
        public static string TrimStart(string aThis, string aSubStr) {
            char[] ci = aThis.ToCharArray();
            char[] di = aSubStr.ToCharArray();

            if (aThis.StartsWith(aSubStr)) {
                if (aThis != aSubStr) {
                    char[] oi = new char[ci.Length - di.Length];
                    for (int i = 0; i < ci.Length - di.Length; i++) {
                        oi[i] = ci[i + di.Length];
                    }
                    return oi.ToString();
                }

                return string.Empty;
            }

            throw new ArgumentNullException();
        }

        public static int CompareOrdinalHelper(string strA, int indexA, int countA, string strB, int indexB, int countB) {
            throw new NotImplementedException();
        }

        public static int GetHashCode(string aThis) {
            throw new NotImplementedException("String.GetHashCode()");
        }
    }
}
