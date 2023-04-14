using System.Globalization;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Globalization
{
    [Plug(Target = typeof(CompareInfo))]
    public static class CompareInfoImpl
    {
        public static void Ctor(CompareInfo context, CultureInfo culture)
        {
        }

        //public static int CompareString(
        //    CompareInfo aThis, ReadOnlySpan<char> string1, ReadOnlySpan<char> string2, CompareOptions options) =>
        //    CompareOrdinalIgnoreCase(string1, string2);

        //public static int CompareOrdinalIgnoreCase(ReadOnlySpan<char> string1, ReadOnlySpan<char> string2)
        //{
        //    var minLength = Math.Min(string1.Length, string2.Length);

        //    for (int i = 0; i < minLength; i++)
        //    {
        //        if (string1[i] < string2[i])
        //        {
        //            return -1;
        //        }

        //        if (string1[i] > string2[i])
        //        {
        //            return 1;
        //        }
        //    }

        //    if (string1.Length < string2.Length)
        //    {
        //        return -1;
        //    }

        //    if (string1.Length > string2.Length)
        //    {
        //        return 1;
        //    }

        //    return 0;
        //}

        //public static bool StartsWith(
        //    CompareInfo aThis, string source, string prefix, CompareOptions options) =>
        //    StartsWith(aThis, source.AsSpan(), source.AsSpan(), options);

        //public static bool StartsWith(
        //    CompareInfo aThis, ReadOnlySpan<char> source, ReadOnlySpan<char> prefix, CompareOptions options)
        //{
        //    if (source.Length > prefix.Length)
        //    {
        //        return false;
        //    }

        //    for (int i = 0; i < prefix.Length; i++)
        //    {
        //        if (source[i] != prefix[i])
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
    }
}