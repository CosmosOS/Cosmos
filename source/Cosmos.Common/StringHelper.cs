using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common
{
    using Cosmos.Debug.Kernel;

    public static class StringHelper
    {
        internal static Debugger mDebugger = new Debugger("Common", "String Helpers");

        internal enum StringComparisonResultEnum
        {
            Less = -1,

            Equal = 0,

            Greater = 1
        }

        public static string GetCharArrayString(char[] aArray)
        {
            if (aArray == null)
            {
                return string.Empty;
            }

            if (aArray.Length == 0)
            {
                return string.Empty;
            }

            string xString = string.Empty;
            for (int i = 0; i < aArray.Length; i++)
            {
                //xString = string.Concat(xString, aArray[i].ToString());
            }

            return xString;
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
            int xValue = aValue;

            if (aValue < 0)
            {
                xValue *= -1;
            }

            if (aValue == 0)
            {
                xResult = string.Concat(xResult, "0");
            }
            else
            {
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

        public static string GetNumberString(long aValue)
        {
            string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string xResult = string.Empty;
            long xValue = aValue;

            if (aValue < 0)
            {
                xValue *= -1;
            }

            if (aValue == 0)
            {
                xResult = string.Concat(xResult, "0");
            }
            else
            {
                while (xValue > 0)
                {
                    long xValue2 = xValue % 10;
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

        public static int Compare(
            string aString1,
            int aIndex1,
            string aString2,
            int aIndex2,
            int aLength1,
            int aLength2)
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
