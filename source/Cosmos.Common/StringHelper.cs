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

        public static string GetNumberString(uint aValue, bool aIsNegative)
        {
            if (aValue == 0)
            {
                if (aIsNegative)
                {
                    return "-0";
                }
                else
                {
                    return "0";
                }
            }
            const string xDigits = "0123456789";
            char[] xResultChars = new char[11];
            int xCurrentPos = 10;
            while (aValue > 0)
            {
                byte xPos = (byte)(aValue % 10);
                aValue /= 10;
                xResultChars[xCurrentPos] = xDigits[xPos];
                xCurrentPos -= 1;
            }
            if (aIsNegative)
            {
                xResultChars[xCurrentPos] = '-';
                xCurrentPos -= 1;
            }
            return new string(xResultChars, xCurrentPos + 1, 10 - xCurrentPos);
        }

        public static string GetNumberString(int aValue)
        {
            bool xIsNegative = false;
            if (aValue < 0)
            {
                xIsNegative = true;
                aValue *= -1;
            }
            var xResult = GetNumberString((uint)aValue, xIsNegative);
            if (xResult == null)
            {
                return GetNumberString((uint)aValue, xIsNegative);
            }
            return xResult;
        }

        public static int GetStringToNumber(string aString)
        {
            int xNumber = 0;
            if (!string.IsNullOrWhiteSpace(aString))
            {
                for (int i = aString.Length - 1; i >= 0; i--)
                {
                    char xC = aString[i];
                    if (char.IsDigit(xC))
                    {
                        int xValue = xC - '0';
                        xValue = xValue * (int)Math.Pow(10, i);
                        xNumber += xValue;
                    }
                    else if (xC == '-')
                    {
                        xNumber *= -1;
                    }
                    else if (xC == '.')
                    {
                        return xNumber;
                    }
                    else
                    {
                        throw new FormatException("The string parameter is not a number and is invalid.");
                    }
                }
            }
            return xNumber;
        }
    }
}