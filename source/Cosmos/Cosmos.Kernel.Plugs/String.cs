using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(string))]
    public class String {
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

        public static string Concat(string aStrA,
                                    string aStrB) {
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

        public static bool EndsWith(string aThis, string aSubStr, StringComparison aComparison) {
            int xLastIdx = aThis.Length - aSubStr.Length - 1;
            for(int i = 0; i < aSubStr.Length; i++) {
                if(aThis[xLastIdx+i] != aSubStr[i]) {
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
                                     int aStartIndex) {
            int xResult = -1;
            for (int i = 0; i < aSeparators.Length; i++) {
                var xValue = aThis.IndexOf(aSeparators[i],
                                           aStartIndex);
                if (xValue < xResult || xResult == -1) {
                    xResult = xValue;
                }
            }
            return xResult;
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

        public static string Substring(string aThis,
                                       int aStartIndex,
                                       int aCount) {
            var xChars = new char[aCount];
            for (int i = 0; i < aCount; i++) {
                xChars[i] = aThis[aStartIndex + i];
            }
            return new string(xChars);
        }

        private static string[] Split(string aThis,
                                      char[] aSeparators,
                                      StringSplitOptions aSplitOptions) {
            List<string> xResult = new List<string>();
            int xCurPos = 0;
            if (CharArrayContainsChar(aSeparators,
                                      aThis[0])) {
                xCurPos = 1;
                if (aSplitOptions == StringSplitOptions.None) {
                    xResult.Add("");
                }
            }
            while (xCurPos < aThis.Length) {
                int xNextPos = IndexOfAny(aThis,
                                          aSeparators,
                                          xCurPos);
                if (xNextPos == -1) {
                    xResult.Add(aThis.Substring(xCurPos));
                    break;
                }
                if (xNextPos == xCurPos) {
                    if (aSplitOptions == StringSplitOptions.None) {
                        xResult.Add("");
                    }
                    xCurPos = xNextPos + 1;
                }
                xResult.Add(aThis.Substring(xCurPos,
                                            xNextPos - xCurPos));
                xCurPos = xNextPos + 1;
            }
            return xResult.ToArray();
        }
    }
}