using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(TargetName="System.Number, mscorlib", IsMicrosoftdotNETOnly=true)] // not sure about FQN or not, also have a look at the param name
    public static class NumberImpl
    {
        public static string FormatInt32(int aInt, string aStr, NumberFormatInfo aFormat)
        {
            bool xIsNegative = false;
            if (aInt < 0)
            {
                xIsNegative = true;
                aInt *= -1;
            }
            var xResult = UInt32Impl2.GetNumberString((uint)aInt, xIsNegative);
            return xResult;
        }
        public static string FormatDouble(double aInt, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatDouble";
        }

        public static string FormatInt64(long aValue, string aStr, NumberFormatInfo aFormat) {
          return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatInt64";
        }
 
      public static string FormatUInt64(ulong aValue, string aStr, NumberFormatInfo aFormat) {
          return "fix me in Cosmos.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatUInt64";
        }
    }
}