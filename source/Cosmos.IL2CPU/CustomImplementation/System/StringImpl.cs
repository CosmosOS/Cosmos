using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
    [Plug(Target = typeof(String))]
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
            return ConcatArray(new string[] { str0, str1, str2 }, str0.Length + str1.Length + str2.Length);
        }
        public static string Concat(string str0, string str1)
        {
            return ConcatArray(new string[] { str0, str1 }, str0.Length + str1.Length);
        }
        public static string Concat(string str0, string str1, string str2, string str3)
        {
            return ConcatArray(new string[] { str0, str1, str2, str3 }, str0.Length + str1.Length + str2.Length + str3.Length);
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
            return Concat(obj0.ToString(), obj1.ToString(), obj2.ToString(), obj3.ToString());
        }
        //Array concatenation plugs
        public static string Concat(params string[] values)
        {
            int len = 0;
            for (int i = 0; i < values.Length; i++)
            {
                len += values[i].Length;
            }
            return ConcatArray(values, len);
        }
        public static string Concat(params object[] args)
        {
            string[] values = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                values[i] = args[i].ToString();
            }
            return Concat(values);
        }
        public static string ConcatArray(string[] values, int totalLength)
        {

            char[] xResult = new char[totalLength];
            int xCurPos = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var xStr = values[i];
                for (int j = 0; j < xStr.Length; j++)
                {
                    xResult[xCurPos] = xStr[j];
                    xCurPos++;
                }
            }
            return new String(xResult);
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
                for (int i = 0; i < aThis.Length; i++)
                    cs[i] = aThis[i];

                for (int i = aThis.Length; i < totalWidth; i++)
                    cs[i] = paddingChar;
            }
            else
            {
                int offset = totalWidth - aThis.Length;
                for (int i = 0; i < aThis.Length; i++)
                    cs[i + offset] = aThis[i];

                for (int i = 0; i < offset; i++)
                    cs[i] = paddingChar;
            }

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos)
        {
            char[] cs = new char[aThis.Length - startpos];
            int j = 0;
            for (int i = startpos; i < aThis.Length; i++)
                cs[j++] = aThis[i];

            return new string(cs);
        }

        public static string Substring(string aThis, int startpos, int length)
        {
            if (startpos + length > aThis.Length)
                length = aThis.Length - startpos;

            char[] cs = new char[length];

            int j = 0;
            for (int i = startpos; i < startpos + length; i++)
                cs[j++] = aThis[i];

            return new string(cs);
        }

        public static string Replace(string aThis, char oldValue, char newValue)
        {
            char[] cs = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] != oldValue)
                    cs[i] = aThis[i];
                else
                    cs[i] = newValue;
            }

            return new string(cs);
        }

        // HACK: We need to redo this once char support is complete (only returns 0, -1).
        public static int CompareTo(string aThis, string other)
        {
            if (aThis.Length != other.Length)
                return -1;
            for (int i = 0; i < aThis.Length; i++)
                if (aThis[i] != other[i])
                    return -1;
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
                    { //Match found
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

    }
}