//#define COSMOSDEBUG
using System;
using System.Globalization;
using Cosmos.Common;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(string))]
    public static class StringImpl
    {
        internal static Debugger mDebugger = new Debugger("Core", "String Plugs");

        public static unsafe void Ctor(
            string aThis,
            char* aChars,
            [FieldAccess(Name = "System.String System.String.Empty")] ref string aStringEmpty,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aStringLength,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            mDebugger.SendInternal("String.Ctor(string, char*)");

            aStringEmpty = "";
            while (*aChars != '\0')
            {
                aChars++;
                aStringLength++;
            }
            for (int i = 0; i < aStringLength; i++)
            {
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

        /*
         * These 2 unsafe string Ctor are only "stubs" implemented because Encoding needed them existing but our implementation is not
         * using them.
         */
        public unsafe static void Ctor(string aThis, sbyte* aValue)
        {
            throw new NotImplementedException("String Ctor(sbyte ptr '\0' terminated)");
        }

        public unsafe static void Ctor(string aThis, sbyte* aValue, int aStartIndex, int aLength)
        {
            throw new NotImplementedException("String Ctor(sbyte ptr with lenght)");
        }

        public static unsafe int get_Length(
            [ObjectPointerAccess] uint* aThis,
            [FieldAccess(Name = "System.Int32 System.String.m_stringLength")] ref int aLength)
        {
            return aLength;
        }

        public static unsafe char get_Chars(
            [ObjectPointerAccess] uint* aThis,
            int aIndex,
            [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            return *(aFirstChar + aIndex);
        }



        public static bool IsAscii(string aThis)
        {
            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] >= 0x80)
                {
                    return false;
                }
            }
            return true;
        }

        public static string Format(string aFormat, object aArg0)
        {
            if (aArg0 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArg0);
        }

        public static string Format(string aFormat, object aArg0, object aArg1)
        {
            if (aFormat == null)
            {
                throw new ArgumentNullException(nameof(aFormat));
            }
            if (aArg0 == null)
            {
                throw new ArgumentNullException(nameof(aArg0));
            }
            if (aArg1 == null)
            {
                throw new ArgumentNullException(nameof(aArg1));
            }

            return FormatHelper(null, aFormat, aArg0, aArg1);
        }

        public static string Format(string aFormat, object aArg0, object aArg1, object aArg2)
        {
            if (aArg0 == null || aArg1 == null || aArg2 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArg0, aArg1, aArg2);
        }

        public static string Format(string aFormat, params object[] aArgs)
        {
            if (aArgs == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(null, aFormat, aArgs);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0)
        {
            if (aArg0 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0, object aArg1)
        {
            if (aArg0 == null | aArg1 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0, aArg1);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, object aArg0, object aArg1, object aArg2)
        {
            if (aArg0 == null | aArg1 == null || aArg2 == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArg0, aArg1, aArg2);
        }

        public static string Format(IFormatProvider aProvider, string aFormat, params object[] aArgs)
        {
            if (aArgs == null)
            {
                throw new ArgumentNullException(aFormat == null ? "aFormat" : "aArgs");
            }

            return FormatHelper(aProvider, aFormat, aArgs);
        }

        internal static string FormatHelper(IFormatProvider aFormatProvider, string aFormat, params object[] aArgs)
        {
            char[] xCharArray = aFormat.ToCharArray();
            string xFormattedString = string.Empty, xStaticString;
            bool xFoundPlaceholder = false, xParamNumberDone = true;
            int xStartParamNumber = -1, xEndParamNumber = -1, xLastPlaceHolder = 0;

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
                        mDebugger.SendInternal("Found closing placeholder.");
                        if (xEndParamNumber < 0)
                        {
                            xEndParamNumber = i;
                        }
                        string xParamNumber = aFormat.Substring(xStartParamNumber, xEndParamNumber - xStartParamNumber);
                        mDebugger.SendInternal("Calling StringHelper.GetStringToNumber");
                        mDebugger.SendInternal(xParamNumber);
                        int xParamIndex = StringHelper.GetStringToNumber(xParamNumber);
                        mDebugger.SendInternal("Converted paramindex to a number.");
                        if ((xParamIndex < aArgs.Length) && (aArgs[xParamIndex] != null))
                        {
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
                    }
                    else if (xCharArray[i] == ':')
                    {
                        xParamNumberDone = true;
                        xEndParamNumber = i;
                        // TODO: Need to handle different formats. (X, N, etc)
                    }
                    else if ((char.IsDigit(xCharArray[i])) && (!xParamNumberDone))
                    {
                        mDebugger.SendInternal("Getting param number.");
                        if (xStartParamNumber < 0)
                        {
                            xStartParamNumber = i;
                        }
                    }
                }
                else if (xCharArray[i] == '{')
                {
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

        public static bool StartsWith(string aThis, string aSubstring, StringComparison aComparison)
        {
            char[] di = aThis.ToCharArray();
            char[] ci = aSubstring.ToCharArray();
            if (aSubstring.Length > aThis.Length)
            {
                return false;
            }
            for (int i = 0; i < ci.Length; i++)
            {
                if (di[i] != ci[i])
                {
                    return false;

                }
            }
            return true;
        }

        public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded)
        {
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

        //        System.Int32  System.String.IndexOf(System.String, System.Int32, System.Int32, System.StringComparison)

        public static int IndexOf(string aThis, string aSubstring, int aIdx, int aLength, StringComparison aComparison)
        {
            return boyerMooreHorsepool(aSubstring, aThis.Substring(aIdx, aLength));
        }

        public static bool Contains(string aThis, string value)
        {
            if (value.Length == aThis.Length)
            {
                if (value == aThis)
                {
                    return true;
                }

                return false;
            }

            if (value.Length > aThis.Length)
            {
                return false;
            }

            var di = aThis.ToCharArray();
            var ci = value.ToCharArray();

            for (int i = 0; i + value.Length <= aThis.Length; i++)
            {
                if (di[i] == ci[0])
                {
                    var equals = true;

                    for (int j = 1; j < value.Length; j++)
                    {
                        if (di[i + j] != ci[j])
                        {
                            equals = false;
                        }
                    }

                    if (equals)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool EndsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture)
        {
            return EndsWith(aThis, aSubStr, StringComparison.CurrentCulture);
        }

        public static bool EndsWith(string aThis, string aSubStr, StringComparison aComparison)
        {
            char[] di = aThis.ToCharArray();
            char[] ci = aSubStr.ToCharArray();
            if (aThis.Length == aSubStr.Length)
            {
                if (aThis == aSubStr)
                {
                    return true;
                }
                return false;
            }
            else if (aThis.Length < aSubStr.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < ci.Length; i++)
                {
                    if (di[aThis.Length - aSubStr.Length + i] != ci[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool Equals(string aThis, string aThat, StringComparison aComparison)
        {
            // TODO: implement
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
                throw new ArgumentNullException(nameof(aSeparators));
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
            mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrA|aIndexA = {aStrA}|{aIndexA}, aStrB|aIndexB = {aStrB}|{aIndexB}, aCount = {aCount}");
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aCount));
            }

            if (aIndexA < 0 || aIndexA > aStrA.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(aIndexA));
            }

            if (aIndexB < 0 || aIndexB > aStrB.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(aIndexB));
            }

            if (aStrA == null)
            {
                mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA is null");
                if (aStrB == null)
                {
                    mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is null");
                    mDebugger.SendInternal($"nativeCompareOrdinalEx : returning 0");
                    return 0;
                }
                mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is not null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : returning -1");
                return -1;
            }
            if (aStrB == null)
            {
                mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA is not null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : aStrB is null");
                mDebugger.SendInternal($"nativeCompareOrdinalEx : returning 1");
                return 1;
            }
            int xLengthA = Math.Min(aStrA.Length, aCount - aIndexA);
            int xLengthB = Math.Min(aStrB.Length, aCount - aIndexB);
            //mDebugger.SendInternal($"nativeCompareOrdinalEx : xLengthA = {xLengthA}");
            //mDebugger.SendInternal($"nativeCompareOrdinalEx : xLengthB = {xLengthB}");

            if (xLengthA == xLengthB && aIndexA == aIndexB && ReferenceEquals(aStrA, aStrB))
            {
                mDebugger.SendInternal("nativeCompareOrdinalEx : xLengthA == xLengthB && aIndexA == aIndexB && aStrA is the same object asaStrB, returning 0");
                return 0;
            }

            int xResult = 0;
            if (xLengthA != xLengthB)
            {
                xResult = xLengthA - xLengthB;
                mDebugger.SendInternal("nativeCompareOrdinalEx : xLengthA != xLengthB, returning " + xResult);
            }

            for (int i = 0; i < xLengthA; i++)
            {
                if (aStrA != aStrB)
                {
                    xResult = (byte)aStrA[i] - (byte)aStrB[i];
                    mDebugger.SendInternal("nativeCompareOrdinalEx : aStrA[i] != aStrB[i], returning " + xResult);
                    return xResult;
                }
            }

            mDebugger.SendInternal("nativeCompareOrdinalEx (end of func) : aStrA[i] != aStrB[i], returning " + xResult);
            return xResult;
        }

        public static bool StartsWith(string aThis, string aSubStr, bool aIgnoreCase, CultureInfo aCulture)
        {
            char[] di = aThis.ToCharArray();
            char[] ci = aSubStr.ToCharArray();
            if (aSubStr.Length > aThis.Length)
            {
                return false;
            }
            for (int i = 0; i < ci.Length; i++)
            {
                if (di[i] != ci[i])
                {
                    return false;

                }
            }
            return true;
        }

        //public static string Remove(string aThis, int aStart, int aCount)
        //{
        //    return aThis.Substring(0, aStart) + aThis.Substring(aStart + aCount, aThis.Length - (aStart + aCount));
        //}

        public static string Replace(string aThis, string oldValue, string newValue)
        {
            int skipOffset = 0;

            while (aThis.Substring(skipOffset).IndexOf(oldValue) != -1)
            {
                int xIndex = aThis.Substring(skipOffset).IndexOf(oldValue) + skipOffset;
                aThis = aThis.Remove(xIndex, oldValue.Length);
                aThis = aThis.Insert(xIndex, newValue);

                skipOffset = xIndex + newValue.Length;
                if (skipOffset > aThis.Length)
                {
                    break;
                }
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

        public static string FastAllocateString(int aLength)
        {
            return new string(new char[aLength]);
        }

        [PlugMethod(IsOptional = true)]
        public static string TrimStart(string aThis, string aSubStr)
        {
            char[] ci = aThis.ToCharArray();
            char[] di = aSubStr.ToCharArray();

            if (aThis.StartsWith(aSubStr))
            {
                if (aThis != aSubStr)
                {
                    char[] oi = new char[ci.Length - di.Length];
                    for (int i = 0; i < ci.Length - di.Length; i++)
                    {
                        oi[i] = ci[i + di.Length];
                    }
                    return oi.ToString();
                }

                return string.Empty;
            }

            throw new ArgumentNullException();
        }

        internal static unsafe char *GetFirstChar(string aThis, [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            return aFirstChar;
        }

        private static unsafe int FastCompareStringHelper(uint* strAChars, int countA, uint* strBChars, int countB)
        {
            int count = (countA < countB) ? countA : countB;

#if BIT64
            long diff = (long)((byte*)strAChars - (byte*)strBChars);
#else
            int diff = (int)((byte*)strAChars - (byte*)strBChars);
#endif

#if BIT64
            int alignmentA = (int)((long)strAChars) & (sizeof(IntPtr) - 1);
            int alignmentB = (int)((long)strBChars) & (sizeof(IntPtr) - 1);

            if (alignmentA == alignmentB)
            {
                if ((alignmentA == 2 || alignmentA == 6) && (count >= 1))
                {
                    char* ptr2 = (char*)strBChars;

                    if ((*((char*)((byte*)ptr2 + diff)) - *ptr2) != 0)
                        return ((int)*((char*)((byte*)ptr2 + diff)) - (int)*ptr2);

                    strBChars = (uint*)(++ptr2);
                    count -= 1;
                    alignmentA = (alignmentA == 2 ? 4 : 0);
                }

                if ((alignmentA == 4) && (count >= 2))
                {
                    uint* ptr2 = (uint*)strBChars;

                    if ((*((uint*)((byte*)ptr2 + diff)) - *ptr2) != 0)
                    {
                        char* chkptr1 = (char*)((byte*)strBChars + diff);
                        char* chkptr2 = (char*)strBChars;

                        if (*chkptr1 != *chkptr2)
                            return ((int)*chkptr1 - (int)*chkptr2);
                        return ((int)*(chkptr1 + 1) - (int)*(chkptr2 + 1));
                    }
                    strBChars = ++ptr2;
                    count -= 2;
                    alignmentA = 0;
                }

                if ((alignmentA == 0))
                {
                    while (count >= 4)
                    {
                        long* ptr2 = (long*)strBChars;

                        if ((*((long*)((byte*)ptr2 + diff)) - *ptr2) != 0)
                        {
                            if ((*((uint*)((byte*)ptr2 + diff)) - *(uint*)ptr2) != 0)
                            {
                                char* chkptr1 = (char*)((byte*)strBChars + diff);
                                char* chkptr2 = (char*)strBChars;

                                if (*chkptr1 != *chkptr2)
                                    return ((int)*chkptr1 - (int)*chkptr2);
                                return ((int)*(chkptr1 + 1) - (int)*(chkptr2 + 1));
                            }
                            else
                            {
                                char* chkptr1 = (char*)((uint*)((byte*)strBChars + diff) + 1);
                                char* chkptr2 = (char*)((uint*)strBChars + 1);

                                if (*chkptr1 != *chkptr2)
                                    return ((int)*chkptr1 - (int)*chkptr2);
                                return ((int)*(chkptr1 + 1) - (int)*(chkptr2 + 1));
                            }
                        }
                        strBChars = (uint*)(++ptr2);
                        count -= 4;
                    }
                }

                {
                    char* ptr2 = (char*)strBChars;
                    while ((count -= 1) >= 0)
                    {
                        if ((*((char*)((byte*)ptr2 + diff)) - *ptr2) != 0)
                            return ((int)*((char*)((byte*)ptr2 + diff)) - (int)*ptr2);
                        ++ptr2;
                    }
                }
            }
            else
#endif // BIT64
            {
#if BIT64
                if (Math.Abs(alignmentA - alignmentB) == 4)
                {
                    if ((alignmentA == 2) || (alignmentB == 2))
                    {
                        char* ptr2 = (char*)strBChars;

                        if ((*((char*)((byte*)ptr2 + diff)) - *ptr2) != 0)
                            return ((int)*((char*)((byte*)ptr2 + diff)) - (int)*ptr2);
                        strBChars = (uint*)(++ptr2);
                        count -= 1;
                    }
                }
#endif // BIT64

                // Loop comparing a DWORD at a time.
                // Reads are potentially unaligned
                while ((count -= 2) >= 0)
                {
                    if ((*((uint*)((byte*)strBChars + diff)) - *strBChars) != 0)
                    {
                        char* ptr1 = (char*)((byte*)strBChars + diff);
                        char* ptr2 = (char*)strBChars;
                        if (*ptr1 != *ptr2)
                            return ((int)*ptr1 - (int)*ptr2);
                        return ((int)*(ptr1 + 1) - (int)*(ptr2 + 1));
                    }
                    ++strBChars;
                }

                int c;
                if (count == -1)
                    if ((c = *((char*)((byte*)strBChars + diff)) - *((char*)strBChars)) != 0)
                        return c;
            }

            return countA - countB;
        }

        public static unsafe int CompareOrdinalHelper(string strA, int indexA, int countA, string strB, int indexB, int countB)
        {
#if false
            // Set up the loop variables.
            fixed (char* pStrA = strA, pStrB = strB)
            {
                char* strAChars = pStrA + indexA;
                char* strBChars = pStrB + indexB;
                return FastCompareStringHelper((uint*)strAChars, countA, (uint*)strBChars, countB);
            }
#endif

            /* Totally managed version but requires changes to IL2CPU to work */
#if true
            // Please note that Argument validation should be handled by callers.
            int count = (countA < countB) ? countA : countB;
            int xResult = 0;

            strA = strA.Substring(indexA);
            strB = strB.Substring(indexB);

            /*
             * This optimization is not taking effect yet in Cosmos as String.Intern() is not implemented
             */ 
            if (ReferenceEquals(strA, strB))
            {
                mDebugger.SendInternal($"strA ({strA}) is the same object of strB ({strB}) returning 0");
                return 0;
            }
            else
            {
                mDebugger.SendInternal($"strA ({strA}) is NOT the same object of StrB ({strB})");
            }

            for (int i = 0; i < count; i++)
            {
                int a = strA[i];
                int b = strB[i];
                //xResult = strA[i] - strB[i];
                xResult = a - b;
                // Different characters we have finished
                if (xResult != 0)
                {
                    break;
                }
            }

            return xResult;
#endif
        }

        public static int CompareOrdinalHelperIgnoreCase(string strA, int indexA, int countA, string strB, int indexB, int countB)
        {
            return CompareOrdinalHelper(strA.ToLower(), indexA, countA, strB.ToLower(), indexB, countB);
        }

        /*
         * This disables Marvin Hashing end enable the legacy not randomized version of String HashCode.
         * We could have ported Marvin to Cosmos as in CoreRt does exists a managed implementation but it will be used
         * by String.GetHashCode() directly in Net Core 2.1 so better to wait. The only problem is that it needs Unsafe to work.
         */
        public static bool InternalUseRandomizedHashing()
        {
            return false;
        }

        public static int InternalMarvin32HashString(string s, int strLen, long additionalEntropy)
        {
            throw new NotImplementedException("String.InternalMarvin32HashString()");
        }

        /* It is not really needed to plug GetHashCode! */

        public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
        {
            // TODO Exceptions

            int lengthA = Math.Min(length, strA.Length - indexA);
            int lengthB = Math.Min(length, strB.Length - indexB);

            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return CompareOrdinalHelper(strA, indexA, lengthA, strB, indexB, lengthB);

                case StringComparison.OrdinalIgnoreCase:
                    return CompareOrdinalHelperIgnoreCase(strA, indexA, lengthA, strB, indexB, lengthB);

                default:
                    throw new ArgumentException("Not Supported StringComparison");
            }
        }


        public unsafe static int nativeCompareOrdinalIgnoreCaseWC(string strA, sbyte* strBBytes)
        {
            throw new NotImplementedException("nativeCompareOrdinalIgnoreCaseWC");
        }


    }
}
