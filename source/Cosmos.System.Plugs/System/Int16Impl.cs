﻿using System;
using System.Globalization;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(short))]
    public static class Int16Impl
    {
        //[PlugMethod(Signature = "System_String___System_Int16_ToString____")]
        public static string ToString(ref short aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            throw new NotImplementedException();
        }

        public static Int16 Parse(string s)
        {
            const string digits = "0123456789";
            Int16 result = 0;

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
                Int16 ind = (Int16)digits.IndexOf(s[i]);
                if (ind == -1)
                {
                    global::System.Console.Write("Digit '");
                    global::System.Console.Write(s[i]);
                    global::System.Console.WriteLine("' not found!");
                    throw new FormatException();
                }
                result = (Int16)((result * 10) + ind);
            }

            if (neg) result *= -1;

            return result;
        }
    }
}