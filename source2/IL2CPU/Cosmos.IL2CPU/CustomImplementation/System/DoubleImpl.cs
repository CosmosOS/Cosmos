using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(Double))]
    public static class DoubleImpl
    {
        public static string ToString(ref double aThis)
        {
            if (aThis == 0.0)
            {
                return "0";
            }

            double val = aThis;

            byte[] doubleBytes = BitConverter.GetBytes(val);
            Int64 hexVal = BitConverter.ToInt64(doubleBytes, 0);
            Int64 mantissa, intPart = 0, fracPart = 0;
            Int32 exp2;

            exp2 = (int)((hexVal >> 52) & 0x07FF) - 1023;
            mantissa = (hexVal & 0x1FFFFFFFFFFFFF) | 0x10000000000000;

            if (exp2 >= 63)
            {
                return "Double Overrange";
            }
            else if (exp2 < -52)
            {
                return "Double Underrange";
            }
            else if (exp2 >= 52)
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
            result += ((UInt64)intPart).ToString();
            int used_digits = ((UInt64)intPart).ToString().Length;
            if (fracPart == 0)
            {
                return result;
            }
			result += ".";

            if (used_digits >= 15)
            {
                used_digits = 14;
            }
            for (int m = used_digits; m < 15; m++)
            {
                fracPart = (fracPart << 3) + (fracPart << 1);
                char p = (char)(((fracPart >> 53) & 0xFF) + '0');
                result += p;

                fracPart &= 0x1FFFFFFFFFFFFF;
            }
            fracPart = (fracPart << 3) + (fracPart << 1);
            char remain = (char)((fracPart >> 53) + '0');
            if ((remain > '5') && (result[result.Length - 1] > '0'))
            {
                char[] answer = result.ToCharArray();
                int digitPos = answer.Length - 1;
                char digit = result[digitPos];
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

                result = new String(answer);
            }

            while (result[result.Length - 1] == '0')
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
    }
}