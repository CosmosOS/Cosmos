using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System.Globalization {
    [Plug(Target=typeof(NumberFormatInfo))]
    public static class NumberFormatInfoImpl {
        public static NumberFormatInfo GetInstance(IFormatProvider aProvider) {
            Console.WriteLine("NumberFormatInfo.GetInstance(IFormatProvider) is not working!");
            return null;
        }
    }
}