using System.Globalization;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Globalization
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

        public static void CCtor()
        {
        }
    }
}
