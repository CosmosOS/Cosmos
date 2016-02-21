using System.Globalization;

using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Globalization
{
    [Plug(Target = typeof(CultureInfo))]
    public static class CultureInfoImpl
    {
        static Debugger mDebugger = new Debugger("Core", "Compare Info Plug");

        public static void Ctor(CultureInfo aThis, string name, bool useUserOverride)
        {
        }

        public static bool get_UseUserOverride(CultureInfo aThis)
        {
            return false;
        }

        public static CultureInfo get_CurrentCulture()
        {
            return new CultureInfo("en-us");
        }

        public static CultureInfo get_InvariantCulture()
        {
            return null;
        }

        public static void CCtor()
        {
        }

        public static CultureInfo GetCultureInfo(string aName)
        {
            return null;
        }

        public static bool Equals(CultureInfo aThis, object aThat)
        {
            return ReferenceEquals(aThis, aThat);
        }
    }
}
