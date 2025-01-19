using System;
using System.Globalization;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Globalization
{
    [Plug(Target = typeof(CultureInfo))]
    public static class CultureInfoImpl
    {
        public static void CCtor(CultureInfo aThis, [FieldAccess(Name = "System.Boolean System.Globalization.CultureInfo._isInherited")] ref bool aIsInherited)
        {
            aIsInherited = false;
        }

        public static void Ctor(CultureInfo aThis, string name)
        {
        }

        public static CultureInfo get_CurrentCulture() => CultureInfo.InvariantCulture;

        public static int GetHashCode(CultureInfo aThis)
        {
            throw new NotImplementedException("CultureInfo.GetHashCode()");
        }

        public static bool Equals(CultureInfo aThis, object value)
        {
            throw new NotImplementedException("CultureInfo.Equals()");
        }

        public static CultureInfo GetCultureInfo(string name) => null;

        public static CultureInfo get_Parent(CultureInfo aThis)
        {
            throw new NotImplementedException();
        }

        public static string GetUserDefaultLocaleName()
        {
            return "GetUserDefaultLocaleName is not implemented";
        }
    }
}
