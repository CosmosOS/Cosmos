using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(Double))]
    public static class DoubleImpl
    {
        public static string ToString(ref double aThis)
        {
            return DoubleImpl2.GetNumberString(aThis);
        }
    }
    public static class DoubleImpl2
    {
        public static string GetNumberString(double aValue)
        {
            //const string xDigits = "0123456789";
            //const char divisor = '.';
            char[] xResultChars= new char[70];
            //BitConverter.(aValue,0);
            return null;
        }
    }
}
