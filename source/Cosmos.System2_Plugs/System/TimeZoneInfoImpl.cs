using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(typeof(TimeZoneInfo))]
    public static class TimeZoneInfoImpl
    {
        public static bool TryConvertIanaIdToWindowsId(string ianaId, bool allocate, out string windowsId)
        {
            windowsId = null;
            return false;
        }

        public static string GetUtcStandardDisplayName()
        {
            return "Coordinated Universal Time";
        }
    }
}
