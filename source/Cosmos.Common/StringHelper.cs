#define COSMOSDEBUG

using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Common
{
    public static class StringHelper
    {
        private static Debugger mDebugger = new Debugger("Common", "StringHelper");

        internal enum StringComparisonResultEnum
        {
            Less = -1,

            Equal = 0,

            Greater = 1
        }

        public static string GetNumberString(uint aValue)
        {
            string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string xResult = string.Empty;

            if (aValue == 0)
            {
                xResult = "0";
            }
            else
            {
                uint xValue = aValue;
                while (xValue > 0)
                {
                    uint xValue2 = xValue % 10;
                    xResult = string.Concat(xChars[xValue2], xResult);
                    xValue /= 10;
                }
            }

            return xResult;
        }

        public static string GetNumberString(int aValue)
        {
            string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string xResult = string.Empty;

            if (aValue == 0)
            {
                xResult = "0";
            }
            else
            {
                int xValue = aValue;

                if (aValue < 0)
                {
                    xValue *= -1;
                }

                while (xValue > 0)
                {
                    int xValue2 = xValue % 10;
                    xResult = string.Concat(xChars[xValue2], xResult);
                    xValue /= 10;
                }
            }

            if (aValue < 0)
            {
                xResult = string.Concat("-", xResult);
            }

            return xResult;
        }

        public static string GetNumberString(ulong aValue, bool aIsNegative)
        {
            const string xDigits = "0123456789";
            char[] xResultChars = new char[21];
            int xCurrentPos = 20;
            if (aValue > 0)
            {
                while (aValue > 0)
                {
                    byte xPos = (byte)(aValue % 10);
                    aValue /= 10;
                    xResultChars[xCurrentPos] = xDigits[xPos];
                    xCurrentPos -= 1;
                }
            }
            else
            {
                xResultChars[xCurrentPos] = '0';
                xCurrentPos -= 1;
            }
            if (aIsNegative)
            {
                xResultChars[xCurrentPos] = '-';
                xCurrentPos -= 1;
            }
            return new String(xResultChars, xCurrentPos + 1, 20 - xCurrentPos);
        }

        public static string GetNumberString(ulong aValue)
        {
            mDebugger.SendInternal("StringHrlprt.GetNumberString");
            mDebugger.SendInternal("aValue =");
            mDebugger.SendInternal(aValue);

            string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string xResult = string.Empty;

            if (aValue == 0)
            {
                xResult = "0";
                mDebugger.SendInternal("xResult =");
                mDebugger.SendInternal(xResult);
            }
            else
            {
                ulong xValue = aValue;
                mDebugger.SendInternal("xValue =");
                mDebugger.SendInternal(xValue);
                while (xValue > 0)
                {
                    ulong xValue2 = xValue % 10;
                    mDebugger.SendInternal("xValue2 =");
                    mDebugger.SendInternal(xValue2);
                    xResult = string.Concat(xChars[xValue2], xResult);
                    mDebugger.SendInternal("xResult =");
                    mDebugger.SendInternal(xResult);
                    xValue /= 10;
                    mDebugger.SendInternal("xValue =");
                    mDebugger.SendInternal(xValue);
                }
            }

            mDebugger.SendInternal("xResult =");
            mDebugger.SendInternal(xResult);
            return xResult;
        }

        public static string GetNumberString(long aValue)
        {
            mDebugger.SendInternal("StringHrlprt.GetNumberString");
            mDebugger.SendInternal("aValue =");
            mDebugger.SendInternal(aValue);

            string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string xResult = string.Empty;
            long xValue = aValue;

            if (aValue == 0)
            {
                xResult = "0";
                mDebugger.SendInternal("xResult =");
                mDebugger.SendInternal(xResult);
            }
            else
            {
                if (aValue < 0)
                {
                    xValue *= -1;
                }

                mDebugger.SendInternal("xValue =");
                mDebugger.SendInternal(xValue);
                while (xValue > 0)
                {
                    Debugger.DoBochsBreak();
                    long xValue2 = xValue % 10;
                    mDebugger.SendInternal("xValue2 =");
                    mDebugger.SendInternal(xValue2);
                    xResult = string.Concat(xChars[xValue2], xResult);
                    mDebugger.SendInternal("xResult =");
                    mDebugger.SendInternal(xResult);
                    xValue /= 10;
                    mDebugger.SendInternal("xValue =");
                    mDebugger.SendInternal(xValue);
                }
            }

            if (aValue < 0)
            {
                xResult = string.Concat("-", xResult);
            }

            mDebugger.SendInternal("xResult =");
            mDebugger.SendInternal(xResult);
            return xResult;
        }

        public static int GetStringToNumber(string aString)
        {
            bool xIsNegative = false;
            int xNumber = 0;
            if (!string.IsNullOrWhiteSpace(aString))
            {
                char[] xCharArray = aString.ToCharArray();
                for (int i = 0; i < xCharArray.Length; i++)
                {
                    if (char.IsDigit(xCharArray[i]))
                    {
                        int xValue = xCharArray[i] - '0';
                        int xMax = xCharArray.Length - 1;
                        for (int j = 0; j < xMax - i; i++)
                        {
                            xValue *= 10;
                        }

                        xNumber += xValue;
                    }
                    else if (xCharArray[i] == '-')
                    {
                        xIsNegative = true;
                    }
                    else if (xCharArray[i] == '.')
                    {
                        break;
                    }
                    else
                    {
                        throw new FormatException("The string parameter is not a number and is invalid.");
                    }
                }
            }

            if (xIsNegative)
            {
                xNumber *= -1;
            }

            return xNumber;
        }

        public static int Compare(string aString1, int aIndex1, string aString2, int aIndex2, int aLength1,int aLength2)
        {
            if (aString1.Length < aString2.Length)
            {
                return (int)StringComparisonResultEnum.Less;
            }
            if (aString1.Length > aString2.Length)
            {
                return (int)StringComparisonResultEnum.Greater;
            }

            for (int i = aString1.Length; i < aString1.Length; i++)
            {
                if (aString1[i] < aString2[i])
                {
                    return (int)StringComparisonResultEnum.Equal;
                }
                if (aString1[i] > aString2[i])
                {
                    return (int)StringComparisonResultEnum.Greater;
                }
            }
            return (int)StringComparisonResultEnum.Equal;
        }
    }
}
