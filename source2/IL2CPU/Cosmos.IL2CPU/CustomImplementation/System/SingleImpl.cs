using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
    [Plug(Target = typeof(Single))]
    public static class SingleImpl
    {
        public static string ToString(ref float aThis)
        {
            if (aThis == 0.0f)
            {
                return "0";
            }

            byte[] singleBytes = BitConverter.GetBytes(aThis);
            Int32 hexVal = BitConverter.ToInt32(singleBytes, 0);
            Int32 mantissa, intPart = 0, fracPart = 0;
            Int32 exp2;

            exp2 = ((hexVal >> 23) & 0xFF) - 127;
            mantissa = (hexVal & 0xFFFFFF) | 0x800000;

            if (exp2 >= 31)
            {
                return "Single Overrange";
            }
            else if (exp2 < -23)
            {
                return "Single Underrange";
            }
            else if (exp2 >= 23)
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
            result += ((UInt32)intPart).ToString();
            int used_digits = ((UInt32)intPart).ToString().Length;
            if (fracPart == 0)
            {
                return result;
            }
			result += ".";

            if (used_digits >= 7)
            {
                used_digits = 6;
            }
            for (int m = used_digits; m < 7; m++)
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