using System;
using Cosmos.Common;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(int))]
public static class Int32Impl
{
    public static string ToString(ref int aThis) => StringHelper.GetNumberString(aThis);

    public static string ToString(ref int aThis, string format)
    {
        if (format.Equals("X"))
        {
            var result = "";

            if (aThis == 0)
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
                    var temp = "";

                    switch (aThis % 16)
                    {
                        case 10:
                            temp = "A";
                            break;
                        case 11:
                            temp = "B";
                            break;
                        case 12:
                            temp = "C";
                            break;
                        case 13:
                            temp = "D";
                            break;
                        case 14:
                            temp = "E";
                            break;
                        case 15:
                            temp = "F";
                            break;
                    }

                    result = temp + result;
                }

                aThis /= 16;
            }

            return result;
        }

        return aThis.ToString();
    }

    public static string ToString(ref int aThis, IFormatProvider provider) => ToString(ref aThis);

    public static string ToString(ref int aThis, string format, IFormatProvider provider) =>
        ToString(ref aThis, format);

    public static int Parse(string s)
    {
        const string digits = "0123456789";
        var result = 0;

        var z = 0;
        var neg = false;

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

        for (var i = z; i < s.Length; i++)
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
