using System;

using Cosmos.Common;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Int32))]
    public static class Int32Impl
    {
        public static string ToString(ref int aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static Int32 Parse(string s)
        {
            const string digits = "0123456789";
            Int32 result = 0;

            int z = 0;
            bool neg = false;

            if (s.Length >= 1)
            {
                if (s[0] == '+') z = 1;
                if (s[0] == '-')
                {
                    z = 1;
                    neg = true;
                }
            }

            for (int i = z; i < s.Length; i++)
            {
                Int32 ind = (Int32)digits.IndexOf(s[i]);
                if (ind == -1)
                {
                    throw new FormatException();
                }
                result = (Int32)((result * 10) + ind);
            }

            if (neg) result *= -1;

            return result;
        }
    }
}
