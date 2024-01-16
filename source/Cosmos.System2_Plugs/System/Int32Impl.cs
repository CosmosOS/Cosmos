using System.Text;
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
            if(format == string.Empty)
            {
                return aThis.ToString();
            }

            ArgumentNullException.ThrowIfNull(format, nameof(format));

            StringBuilder sb = new();

            switch (format[0])
            {
                case 'X':
                    if (aThis == 0)
                    {
                        sb.Append('0');
                        break;
                    }

                    int value = aThis;

                    while (value != 0)
                    {
                        int remainder = value % 16;
                        if (remainder < 10)
                        {
                            sb.Insert(0, remainder);
                        }
                        else
                        {
                            char temp = (char)('A' + (remainder - 10));
                            sb.Insert(0, temp);
                        }

                        value /= 16;
                    }
                    break;
                case 'D':
                    sb.Append(aThis);
                    break;
                default:
                    return aThis.ToString();
            }

            var result = sb.ToString();

            if (format.Length > 1)
            {
                return int.TryParse(format.AsSpan(1), out int number) ? result.PadLeft(number, '0') : aThis.ToString();
            }
            else
            {
                return result;
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
                result = int.Parse(s);
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