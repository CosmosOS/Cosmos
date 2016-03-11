using System;
using System.Globalization;
using Cosmos.Common;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Globalization
{
    [Plug(Target = typeof(global::System.Globalization.CultureInfo))]
    public static class CultureInfoPlug
    {
        public static int GetHashCode(global::System.Globalization.CultureInfo aThis)
        {
            throw new NotImplementedException("CultureInfo.GetHashCode()");
        }
    }
}
