using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System
{
    [Plug(TargetName="System.Number, mscorlib", IsMicrosoftdotNETOnly=true)] // not sure about FQN or not, also have a look at the param name
    public static class NumberImpl
    {
        public static string FormatInt32(int aInt, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Indy.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatInt32";
        }
        public static string FormatDouble(double aInt, string aStr, NumberFormatInfo aFormat)
        {
            return "fix me in Indy.IL2CPU.IL.CustomImplementations.System.NumberImpl.FormatDouble";
        }
    }
}