using System;
using System.Globalization;

namespace Cosmos.IL2CPU.Plugs.System.Globalization {
    [Plug(Target=typeof(NumberFormatInfo))]
    public static class NumberFormatInfoImpl {
        public static NumberFormatInfo GetInstance(IFormatProvider aProvider) {
            //Console.WriteLine("NumberFormatInfo.GetInstance(IFormatProvider) is not working!");
            return null;
        }
    

        public static NumberFormatInfo get_CurrentInfo()
        {
            //Console.WriteLine("NumberFormatInfo.get_CurrentInfo is not working!");
            return null;
        }
    }
}