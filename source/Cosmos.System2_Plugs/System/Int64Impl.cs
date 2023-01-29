using System;

using Cosmos.Common;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(long))]
    public static class Int64Impl
    {
        public static string ToString(ref long aThis) => StringHelper.GetNumberString(aThis);

        public static string ToString(ref long aThis, string format, IFormatProvider provider) => aThis.ToString();

        public static long Parse(string s)
        {
            const string digits = "0123456789";
            var result = 0L;

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
                var ind = digits.IndexOf(s[i]);
                if (ind == -1)
                {
                    throw new FormatException();
                }
                result = result * 10 + ind;
            }

            if (neg)
            {
                result *= -1;
            }

            return result;
        }
    }
}
