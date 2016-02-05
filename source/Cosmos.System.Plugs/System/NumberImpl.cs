using System.Globalization;

using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(TargetName = "System.Number")]
    // not sure about FQN or not, also have a look at the param name
    public static class NumberImpl
    {
        public static string FormatInt32(int aInt, string aStr, NumberFormatInfo aFormat)
        {
            return StringHelper.GetNumberString(aInt);
        }

        public static string FormatDouble(double aInt, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatDouble";
        }

        public static string FormatInt64(long aValue, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatInt64";
        }

        public static string FormatUInt64(ulong aValue, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatUInt64";
        }
    }
}
