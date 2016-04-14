using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(float))]
    public static class SingleImpl
    {
        public static string ToString(ref float aThis)
        {
            if (aThis == 0.0f)
            {
                return "0";
            }

            float xValue = aThis;

            var singleBytes = BitConverter.GetBytes(xValue);
            int hexVal = BitConverter.ToInt32(singleBytes, 0);
            int intPart = 0, fracPart = 0;

            int exp2 = ((hexVal >> 23) & 0xFF) - 127;
            int mantissa = (hexVal & 0xFFFFFF) | 0x800000;

            if (exp2 >= 31)
            {
                return "Single Overrange";
            }

            if (exp2 < -23)
            {
                return "Single Underrange";
            }

            if (exp2 >= 23)
            {
                intPart = mantissa << (exp2 - 23);
            }
            else if (exp2 >= 0)
            {
                intPart = mantissa >> (23 - exp2);
                fracPart = (mantissa << (exp2 + 1)) & 0xFFFFFF;
            }
            else
            {
                fracPart = (mantissa & 0xFFFFFF) >> (-(exp2 + 1));
            }

            string result = "";
            if (hexVal < 0)
            {
                result += "-";
            }
            result += ((uint)intPart).ToString();
            int usedDigits = ((uint)intPart).ToString().Length;
            if (fracPart == 0)
            {
                return result;
            }
            result += ".";

            if (usedDigits >= 7)
            {
                usedDigits = 6;
            }
            for (int m = usedDigits; m < 7; m++)
            {
                fracPart = (fracPart << 3) + (fracPart << 1);
                char p = (char)((fracPart >> 24) + '0');
                result += p;

                fracPart &= 0xFFFFFF;
            }
            fracPart = (fracPart << 3) + (fracPart << 1);
            char remain = (char)((fracPart >> 24) + '0');
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
