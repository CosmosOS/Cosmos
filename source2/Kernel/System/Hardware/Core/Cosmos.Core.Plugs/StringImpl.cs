using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs {
    //TODO: Move this and other FCL plugs to Cosmos.Plugs assembly. some plugs like Console need hardware
    // but these generics ones should be moved, this does not depend on kernel
    [Plug(Target=typeof(string))]
    public class StringImpl {
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

        public static string Concat(string aStrA, string aStrB) {
            char[] xChars = new char[aStrA.Length + aStrB.Length];
            int xPos = 0;
            for (int i = 0; i < aStrA.Length; i++) {
                xChars[xPos++] = aStrA[i];
            }
            for (int i = 0; i < aStrB.Length; i++) {
                xChars[xPos++] = aStrB[i];
            }
            return new global::System.String(xChars);
        }

        public static string Concat(string aStrA,
                                    string aStrB,
                                    string aStrC) {
            if (aStrA == null) {
                aStrA = string.Empty;
            }
            if (aStrB == null) {
                aStrB = string.Empty;
            }
            if (aStrC == null) {
                aStrC = string.Empty;
            }
            char[] xChars = new char[aStrA.Length + aStrB.Length + aStrC.Length];
            int xPos = 0;
            for (int i = 0; i < aStrA.Length; i++) {
                xChars[xPos++] = aStrA[i];
            }
            for (int i = 0; i < aStrB.Length; i++) {
                xChars[xPos++] = aStrB[i];
            }
            for (int i = 0; i < aStrC.Length; i++) {
                xChars[xPos++] = aStrC[i];
            }

            return new global::System.String(xChars);
        }

        public static string Concat(string aStrA,
                                    string aStrB,
                                    string aStrC,
                                    string aStrD) {
            if (aStrA == null) {
                aStrA = string.Empty;
            }
            if (aStrB == null) {
                aStrB = string.Empty;
            }
            if (aStrC == null) {
                aStrC = string.Empty;
            }
            if (aStrD == null) {
                aStrD = string.Empty;
            }
            char[] xChars = new char[aStrA.Length + aStrB.Length + aStrC.Length + aStrD.Length];
            int xPos = 0;
            for (int i = 0; i < aStrA.Length; i++) {
                xChars[xPos++] = aStrA[i];
            }
            for (int i = 0; i < aStrB.Length; i++) {
                xChars[xPos++] = aStrB[i];
            }
            for (int i = 0; i < aStrC.Length; i++) {
                xChars[xPos++] = aStrC[i];
            }
            for (int i = 0; i < aStrD.Length; i++) {
                xChars[xPos++] = aStrD[i];
            }
            return new global::System.String(xChars);
        }

        public static bool Contains(string aThis, string value) {
            if (aThis.IndexOf(value) != -1)
                return true;
            else
                return false;
        }

        public static int IndexOf(string aThis, string aSubStr, int aStart, int aLength, StringComparison aComparison) {
            int xEndIdx = aStart + aLength;
            for (int i = aStart; i < xEndIdx; i++) {
                bool xFound = true;
                for (int j = 0; j < aSubStr.Length; j++) {
                    if (aThis[i + j] != aSubStr[j]) {
                        xFound = false;
                        break;
                    }
                }
                if (xFound) {
                    return i;
                }
            }
            return -1;
        }

        public static bool EndsWith(string aThis, string aSubStr, bool aIgnoreCase, System.Globalization.CultureInfo aCulture) {
            return EndsWith(aThis, aSubStr, StringComparison.CurrentCulture);
        }

        public static bool EndsWith(string aThis, string aSubStr, StringComparison aComparison) {
            if (aSubStr == null)
                throw new ArgumentNullException("aSubStr");

            if (aThis == aSubStr)
                return true;

            if (aSubStr.Length == 0)
                return true;

            int xLastIdx = aThis.Length - aSubStr.Length;
            for (int i = 0; i < aSubStr.Length; i++) {
                if (aThis[xLastIdx + i] != aSubStr[i]) {
                    return false;
                }
            }
            return true;
        }

        //        System.Int32  System.String.IndexOf(System.String, System.Int32, System.Int32, System.StringComparison)

        public static bool Equals(string aThis, string aThat, StringComparison aComparison) {
#warning TODO: implement
            return EqualsHelper(aThis,
                                aThat);
        }

        public static bool EqualsHelper(string aStrA,
                                        string aStrB) {
            return aStrA.CompareTo(aStrB) == 0;
        }

        private static bool CharArrayContainsChar(char[] aArray,
                                                  char aChar) {
            for (int i = 0; i < aArray.Length; i++) {
                if (aArray[i] == aChar) {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOf(string aThis,
                                  string aValue) {
            return aThis.IndexOf(aValue, 0, aThis.Length, StringComparison.CurrentCulture);
        }

        public static int IndexOf(string aThis,
                                  char aSeparator,
                                  int aStartIndex,
                                  int aCount) {
            int xEndIdx = aStartIndex + aCount;
            for (int i = aStartIndex; i < xEndIdx; i++) {
                if (aThis[i] == aSeparator) {
                    return i;
                }
            }
            return -1;
        }

        public static int IndexOfAny(string aThis,
                                     char[] aSeparators,
                                     int aStartIndex,
                                     int aLength) {
            int xResult = -1;
            for (int i = 0; i < aSeparators.Length; i++) {
                var xValue = aThis.IndexOf(aSeparators[i],
                                           aStartIndex,
                                           aLength);
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
            return LastIndexOfAny(aThis, new char[] { aChar }, aStartIndex, aCount);
        }

        public static int LastIndexOfAny(string aThis,
                                         char[] aChars,
                                         int aStartIndex,
                                         int aCount) {
            for (int i = 0; i < aCount; i++) {
                if (CharArrayContainsChar(aChars,
                                          aThis[aStartIndex - i])) {
                    return aStartIndex - i;
                }
            }
            return -1;
        }

        public static int nativeCompareOrdinalEx(string aStrA, int aIndexA, string aStrB, int aIndexB, int aCount) {
            //Just a basic implementation
            if (aStrA == aStrB)
                return 0;
            else
                return -1;
        }

        public static string Replace(string aThis, char oldChar, char newChar) {
            string nString = "";
            for (int i = 0; i < aThis.Length; i++) {
                if (aThis[i] == oldChar)
                    nString += newChar.ToString();
                else
                    nString += aThis[i];
            }

            return nString;
        }

        public static bool StartsWith(string aThis, string aSubStr, StringComparison aComparison) {
            return StartsWith(aThis, aSubStr, true, null);
        }

        public static bool StartsWith(string aThis, string aSubStr, bool aIgnoreCase, System.Globalization.CultureInfo aCulture) {
            for (int i = 0; i < aSubStr.Length; i++) {
                if (aThis[i] != aSubStr[i])
                    return false;
            }
            return true;
        }

        public static string Substring(string aThis,
                                       int aStartIndex,
                                       int aCount) {
            var xChars = new char[aCount];
            for (int i = 0; i < aCount; i++) {
                xChars[i] = aThis[aStartIndex + i];
            }
            return new string(xChars);
        }

        public static string Remove(string aThis, int aStart, int aCount) {
            return aThis.Substring(0, aStart) + aThis.Substring(aStart + aCount, aThis.Length - (aStart + aCount));
        }

        public static string Replace(string aThis, string oldValue, string newValue) {
            while (aThis.IndexOf(oldValue) != -1) {
                int xIndex = aThis.IndexOf(oldValue);
                aThis = aThis.Remove(xIndex, oldValue.Length);
                aThis = aThis.Insert(xIndex, newValue);
            }
            return aThis;
        }

        public static string ToLower(string aThis, System.Globalization.CultureInfo aCulture) {
            return ChangeCasing(aThis, 65, 90, 32);
        }

        public static string ToUpper(string aThis, System.Globalization.CultureInfo aCulture) {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        public static string ToUpper(string aThis)
        {
            return ChangeCasing(aThis, 97, 122, -32);
        }

        private static string ChangeCasing(string aValue, int lowerAscii, int upperAscii, int offset) {
            char[] xChars = new char[aValue.Length];

            for (int i = 0; i < aValue.Length; i++) {
                int xAsciiCode = (int)aValue[i];
                if ((xAsciiCode <= upperAscii) && (xAsciiCode >= lowerAscii))
                    xChars[i] = (char)(xAsciiCode + offset);
                else
                    xChars[i] = aValue[i];
            }

            return new string(xChars);
        }
    }
}
