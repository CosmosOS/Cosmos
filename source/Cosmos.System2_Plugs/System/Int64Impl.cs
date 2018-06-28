using System;
using Cosmos.Common;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Int64))]
    public class Int64Impl
    {
        public static string ToString(ref long aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static Int64 Parse(string s)
        {
            const string digits = "0123456789";
            Int64 result = 0;

            int z = 0;
            bool neg = false;

            if (s.Length >= 1)
            {
                if (s[0] == '+')
                    z = 1;
                if (s[0] == '-')
                {
                    z = 1;
                    neg = true;
                }
            }

            for (int i = z; i < s.Length; i++)
            {
                Int64 ind = (Int64)digits.IndexOf(s[i]);
                if (ind == -1)
                {
                    throw new FormatException();
                }
                result = (Int64)((result * 10) + ind);
            }

            if (neg)
                result *= -1;

            return result;
        }
    }
}
