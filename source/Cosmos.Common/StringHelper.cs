//#define COSMOSDEBUG

using System;
using Cosmos.Debug.Kernel;

namespace Cosmos.Common;

/// <summary>
///     Helper class for working with strings.
/// </summary>
public static class StringHelper
{
    private static readonly Debugger mDebugger = new("Common", "StringHelper");

    /// <summary>
    ///     Parse uint to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <returns>String value.</returns>
    public static string GetNumberString(uint aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(uint)");

        string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        var xResult = String.Empty;

        if (aValue == 0)
        {
            xResult = "0";
        }
        else
        {
            var xValue = aValue;
            while (xValue > 0)
            {
                var xValue2 = xValue % 10;
                xResult = String.Concat(xChars[xValue2], xResult);
                xValue /= 10;
            }
        }

        return xResult;
    }

    /// <summary>
    ///     Parse int to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <returns>String value.</returns>
    public static string GetNumberString(int aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(int)");

        string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        var xResult = String.Empty;

        if (aValue == 0)
        {
            xResult = "0";
        }
        else
        {
            var xValue = aValue;

            if (aValue < 0)
            {
                xValue *= -1;
            }

            while (xValue > 0)
            {
                var xValue2 = xValue % 10;
                xResult = String.Concat(xChars[xValue2], xResult);
                xValue /= 10;
            }
        }

        if (aValue < 0)
        {
            xResult = String.Concat("-", xResult);
        }

        return xResult;
    }

    /// <summary>
    ///     Parse ulong to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <returns>String value.</returns>
    public static string GetNumberString(ulong aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(ulong)");

        string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        var xResult = String.Empty;

        if (aValue == 0)
        {
            xResult = "0";
            mDebugger.SendInternal("xResult =");
            mDebugger.SendInternal(xResult);
        }
        else
        {
            var xValue = aValue;
            mDebugger.SendInternal("xValue =");
            mDebugger.SendInternal(xValue);
            while (xValue > 0)
            {
                var xValue2 = xValue % 10;
                mDebugger.SendInternal("xValue2 =");
                mDebugger.SendInternal(xValue2);
                xResult = String.Concat(xChars[xValue2], xResult);
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

    /// <summary>
    ///     Parse long to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <returns>String value.</returns>
    public static string GetNumberString(long aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(long)");

        string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        var xResult = String.Empty;
        var xValue = aValue;

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
                var xValue2 = xValue % 10;
                mDebugger.SendInternal("xValue2 =");
                mDebugger.SendInternal(xValue2);
                xResult = String.Concat(xChars[xValue2], xResult);
                mDebugger.SendInternal("xResult =");
                mDebugger.SendInternal(xResult);
                xValue /= 10;
                mDebugger.SendInternal("xValue =");
                mDebugger.SendInternal(xValue);
            }
        }

        if (aValue < 0)
        {
            xResult = String.Concat("-", xResult);
        }

        mDebugger.SendInternal("xResult =");
        mDebugger.SendInternal(xResult);
        return xResult;
    }

    /*
     * This implementation could only print values which integer part is < ULONG_MAX, the only solution would have been to use BigInteger but then
     * I'd have an analogous problem: to plug BigInteger.ToString() and I don't know how many other things.
     * We will retain this code for now it is OK for debug purposes and big value should be printed in scientific notation to be readable anyway...
     */
    /// <summary>
    ///     Parse float to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <remarks>aValue integer part must be < ULONG_MAX.</remarks>
    /// <returns>String value.</returns>
    public static string GetNumberString(float aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(float)");

        var singleBytes = BitConverter.GetBytes(aValue);
        var hexVal = BitConverter.ToInt32(singleBytes, 0);

        /* Let's extract the parts that compose our single: sign, exponent and mantissa */
        var isNeg = hexVal >> 31 != 0;
        var exp = (hexVal >> 23) & 0xFF;
        var mantissa = (ulong)(hexVal & 0x7FFFFF);

        ulong intPart = 0, fracPart = 0;

        /* First we handle the special cases INF, NaN, 0 and denormalized float */
        switch (exp)
        {
            /*
             * INF or NaN?
             */
            case 0xFF:
                if (mantissa == 0)
                {
                    if (isNeg)
                    {
                        return "-∞";
                    }

                    return "∞";
                }

                return "NaN";

            /* 0 or denormalized float? */
            case 0x00:
                if (mantissa == 0)
                {
                    return "0";
                }
                /* Denormalized float have always exp -126 */

                exp = -126;

                break;

            /* Normalized float the exponent is unbiased and the implicit leading one is placed in the mantissa */
            default:
                exp -= 127;
                mantissa |= 0x800000;
                break;
        }

        if (exp >= 23)
        {
            intPart = mantissa << (exp - 23);
        }
        else if (exp >= 0)
        {
            intPart = mantissa >> (23 - exp);
            fracPart = (mantissa << (exp + 1)) & 0xFFFFFF;
        }
        else
        {
            fracPart = (mantissa & 0xFFFFFF) >> -(exp + 1);
        }

        var result = "";

        if (isNeg)
        {
            result += "-";
        }

        result += intPart.ToString();
        var usedDigits = intPart.ToString().Length;
        if (fracPart == 0)
        {
            return result;
        }

        result += ".";

        if (usedDigits >= 7)
        {
            usedDigits = 6;
        }

        for (var m = usedDigits; m < 7; m++)
        {
            fracPart = (fracPart << 3) + (fracPart << 1);

            var p = (char)((fracPart >> 24) + '0');
            result += p;

            fracPart &= 0xFFFFFF;
        }

        fracPart = (fracPart << 3) + (fracPart << 1);
        var remain = (char)((fracPart >> 24) + '0');
        if (remain > '5' && result[result.Length - 1] > '0')
        {
            var answer = result.ToCharArray();
            var digitPos = answer.Length - 1;
            var digit = result[digitPos];
            answer[digitPos] = (char)(digit + 1);
            while (answer[digitPos] > '9')
            {
                answer[digitPos] = '0';
                digitPos--;
                digit = result[digitPos];
                if (digit == '.')
                {
                    digitPos--;
                    digit = result[digitPos];
                }

                answer[digitPos] = (char)(digit + 1);
            }

            result = new string(answer);
        }

        while (result[result.Length - 1] == '0')
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }

    /*
     * This implementation could only print values which integer part is < ULONG_MAX, the only solution would have been to use BigInteger but then
     * I'd have an analogous problem: to plug BigInteger.ToString() and I don't know how many other things.
     * We will retain this code for now it is OK for debug purposes and big value should be printed in scientific notation to be readable anyway...
     */
    /// <summary>
    ///     Parse double to string.
    /// </summary>
    /// <param name="aValue">A value to parse.</param>
    /// <remarks>aValue integer part must be < ULONG_MAX.</remarks>
    /// <returns>String value.</returns>
    public static string GetNumberString(double aValue)
    {
        mDebugger.SendInternal("StringHelper.GetNumberString(double)");
        mDebugger.SendInternal("aValue = ");
        mDebugger.SendInternal(aValue);

        var hexVal = BitConverter.DoubleToInt64Bits(aValue);
        mDebugger.SendInternal("hexVal = ");
        mDebugger.SendInternal(hexVal);

        /* Let's extract the parts that compose our double: sign, exponent and mantissa */
        var isNeg = hexVal >> 63 != 0;
        var exp = (int)((hexVal >> 52) & 0x07FF);
        var mantissa = (ulong)(hexVal & 0x0FFFFFFFFFFFFF);
        mDebugger.SendInternal("isNeg = ");
        mDebugger.SendInternal(isNeg.ToString());
        mDebugger.SendInternal("exp = ");
        mDebugger.SendInternal(exp);
        mDebugger.SendInternal("mantissa = ");
        mDebugger.SendInternal(mantissa);

        ulong intPart = 0, fracPart = 0;

        /* First we handle the special cases INF, NaN, 0 and denormalized float */
        switch (exp)
        {
            case 0x07ff:
                if (mantissa == 0)
                {
                    if (isNeg)
                    {
                        return "-∞";
                    }

                    return "∞";
                }

                return "NaN";

            /* 0 or denormalized double? */
            case 0x0000:
                if (mantissa == 0)
                {
                    return "0";
                }
                /* Denormalized float have always exp -1022 */

                exp = -1022;

                break;

            /* Normalized double the exponent is unbiased and the implicit leading one is placed in the mantissa */
            default:
                exp -= 1023;
                mantissa |= 0x10000000000000;
                break;
        }

        if (exp >= 52)
        {
            intPart = mantissa << (exp - 52);
        }
        else if (exp >= 0)
        {
            intPart = mantissa >> (52 - exp);
            fracPart = (mantissa << (exp + 1)) & 0x1FFFFFFFFFFFFF;
        }
        else
        {
            fracPart = (mantissa & 0x1FFFFFFFFFFFFF) >> -(exp + 1);
        }

        var result = "";

        if (isNeg)
        {
            result += "-";
        }

        result += intPart.ToString();
        var usedDigits = result.Length;
        if (fracPart == 0)
        {
            return result;
        }

        result += ".";

        if (usedDigits >= 15)
        {
            usedDigits = 14;
        }

        for (var m = usedDigits; m < 15; m++)
        {
            fracPart = (fracPart << 3) + (fracPart << 1);
            var p = (char)(((fracPart >> 53) & 0xFF) + '0');
            result += p;

            fracPart &= 0x1FFFFFFFFFFFFF;
        }

        fracPart = (fracPart << 3) + (fracPart << 1);
        var remain = (char)((fracPart >> 53) + '0');
        if (remain > '5' && result[result.Length - 1] > '0')
        {
            var answer = result.ToCharArray();
            var digitPos = answer.Length - 1;
            var digit = result[digitPos];
            answer[digitPos] = (char)(digit + 1);
            while (answer[digitPos] > '9')
            {
                answer[digitPos] = '0';
                digitPos--;
                digit = result[digitPos];
                if (digit == '.')
                {
                    digitPos--;
                    digit = result[digitPos];
                }

                answer[digitPos] = (char)(digit + 1);
            }

            result = new string(answer);
        }

        while (result[result.Length - 1] == '0')
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }

    /// <summary>
    ///     Parse string to int.
    /// </summary>
    /// <param name="aString">A string to parse.</param>
    /// <returns>Int value.</returns>
    public static int GetStringToNumber(string aString)
    {
        var xIsNegative = false;
        var xNumber = 0;
        if (!String.IsNullOrWhiteSpace(aString))
        {
            var xCharArray = aString.ToCharArray();
            for (var i = 0; i < xCharArray.Length; i++)
            {
                if (Char.IsDigit(xCharArray[i]))
                {
                    var xValue = xCharArray[i] - '0';
                    var xMax = xCharArray.Length - 1;
                    for (var j = 0; j < xMax - i; i++)
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

    //TODO: remove unused parameters, or at least wrap the function to one without unused parameters(for the sake of backwards compatibility).
    /// <summary>
    ///     Compare two strings lexicographically.
    /// </summary>
    /// <param name="aString1">String to compare.</param>
    /// <param name="aIndex1">unused.</param>
    /// <param name="aString2">String to compare.</param>
    /// <param name="aIndex2">unused.</param>
    /// <param name="aLength1">unused.</param>
    /// <param name="aLength2">unused.</param>
    /// <returns>Int value.</returns>
    public static int Compare(string aString1, int aIndex1, string aString2, int aIndex2, int aLength1, int aLength2)
    {
        if (aString1.Length < aString2.Length)
        {
            return (int)StringComparisonResultEnum.Less;
        }

        if (aString1.Length > aString2.Length)
        {
            return (int)StringComparisonResultEnum.Greater;
        }

        for (var i = aString1.Length; i < aString1.Length; i++)
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

    /// <summary>
    ///     Check if string is numeric.
    /// </summary>
    /// <param name="aString">A string to check if numeric.</param>
    /// <returns>Returns TRUE if string is numeric.</returns>
    public static bool IsNumeric(string aString)
    {
        for (var i = 0; i < aString.Length; i++)
        {
            if (!Char.IsDigit(aString[i]))
            {
                return false;
            }
        }

        return true;
    }

    internal enum StringComparisonResultEnum
    {
        Less = -1,

        Equal = 0,

        Greater = 1
    }
}
