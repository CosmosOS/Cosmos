using System;
using System.Globalization;

using Cosmos.Common;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(short))]
    public static class Int16Impl
    {
        public static string ToString(ref short aThis) => StringHelper.GetNumberString(aThis);

        public static string ToString(ref short aThis, string format, IFormatProvider provider) => aThis.ToString();

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            throw new NotImplementedException();
        }

        public static short Parse(string s)
        {
            const string digits = "0123456789";
            short result = 0;

            int z = 0;
            bool neg = false;

            if (s.Length >= 1)
            {
                if (s[0] == '+')
                {
                    z = 1;
                }

                if (s[0] == '-')
                {
                    z = 1;
                    neg = true;
                }
            }

            for (int i = z; i < s.Length; i++)
            {
                var ind = (short)digits.IndexOf(s[i]);
                if (ind == -1)
                {
                    Console.Write("Digit '");
                    Console.Write(s[i]);
                    Console.WriteLine("' not found!");
                    throw new FormatException();
                }
                result = (short)(result * 10 + ind);
            }

            if (neg)
            {
                result *= -1;
            }

            return result;
        }
    }
}
