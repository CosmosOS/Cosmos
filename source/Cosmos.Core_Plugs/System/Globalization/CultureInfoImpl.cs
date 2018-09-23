using System;
using System.Globalization;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(Target = typeof(CultureInfo))]
    public static class CultureInfoImpl
    {
        public static CultureInfo get_CurrentCulture()
        {
            return null;
        }

        public static CultureInfo get_InvariantCulture()
        {
            return null;
        }

        public static int GetHashCode(CultureInfo aThis)
        {
            throw new NotImplementedException("CultureInfo.GetHashCode()");
        }

        public static bool Equals(CultureInfo aThis, object value)
        {
            throw new NotImplementedException("CultureInfo.Equals()");
        }

        public static void CCtor()
        {
        }
    }
}
