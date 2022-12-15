using System;

using Cosmos.Common;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(int))]
    public static class Int32Impl
    {
        public static string ToString(ref int aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static string ToString(ref int aThis, string format)
        {
            if (format.Equals("X"))
            {
                string result = "";

                if(aThis == 0)
                {
                    result = "0";
                }

                while (aThis != 0)
                {
                    if (aThis % 16 < 10)
                    {
                        result = aThis % 16 + result;
                    }
                    else
                    {
                        string temp = "";

                        switch (aThis % 16)
                        {
                            case 10: temp = "A"; break;
                            case 11: temp = "B"; break;
                            case 12: temp = "C"; break;
                            case 13: temp = "D"; break;
                            case 14: temp = "E"; break;
                            case 15: temp = "F"; break;
                        }

                        result = temp + result;
                    }

                    aThis /= 16;
                }

                return result;
            }
            else
            {
                return aThis.ToString();
            }
        }

        public static string ToString(ref int aThis, IFormatProvider provider) => ToString(ref aThis);

        public static string ToString(ref int aThis, string format, IFormatProvider provider) => ToString(ref aThis, format);

        public static int Parse(string s)
        {
            const string digits = "0123456789";
            var result = 0;

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

        /* .Net Core TryParse is calling Number.TryParse() that does NRE in Cosmos, plugged it for now */
        public static bool TryParse(string s, out int result)
        {
            try
            {
                result = Int32.Parse(s);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }
    }
}
