using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof (double))]
    public static class DoubleImpl
    {
        public static string ToString(ref double aThis)
        {
            if (aThis == 0.0)
            {
                return "0";
            }

            double val = aThis;

            long hexVal = BitConverter.DoubleToInt64Bits(val);
            long mantissa = (hexVal & 0x1FFFFFFFFFFFFF) | 0x10000000000000;
            int exp2 = (int) (((hexVal >> 52) & 0x07FF) - 1023);
            long intPart = 0, fracPart = 0;

            if (exp2 >= 63)
            {
                return "Double Overrange";
            }

            if (exp2 < -52)
            {
                return "Double Underrange";
            }

            if (exp2 >= 52)
            {
                intPart = mantissa << (exp2 - 52);
            }
            else if (exp2 >= 0)
            {
                intPart = mantissa >> (52 - exp2);
                fracPart = (mantissa << (exp2 + 1)) & 0x1FFFFFFFFFFFFF;
            }
            else
            {
                fracPart = (mantissa & 0x1FFFFFFFFFFFFF) >> (-(exp2 + 1));
            }

            string result = "";
            if (hexVal < 0)
            {
                result += "-";
            }
            result += intPart.ToString();
            int usedDigits = result.Length;
            if (fracPart == 0)
            {
                return result;
            }
            result += ".";

            if (usedDigits >= 15)
            {
                usedDigits = 14;
            }
            for (int m = usedDigits; m < 15; m++)
            {
                fracPart = (fracPart << 3) + (fracPart << 1);
                char p = (char) (((fracPart >> 53) & 0xFF) + '0');
                result += p;

                fracPart &= 0x1FFFFFFFFFFFFF;
            }
            fracPart = (fracPart << 3) + (fracPart << 1);
            char remain = (char) ((fracPart >> 53) + '0');
            if ((remain > '5') && (result[result.Length - 1] > '0'))
            {
                char[] answer = result.ToCharArray();
                int digitPos = answer.Length - 1;
                char digit = result[digitPos];
                answer[digitPos] = (char) (digit + 1);
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
                    answer[digitPos] = (char) (digit + 1);
                }

                result = new string(answer);
            }

            while (result[result.Length - 1] == '0')
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
    }
}
