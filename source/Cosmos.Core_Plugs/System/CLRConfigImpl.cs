using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug("System.CLRConfig, System.Private.CoreLib")]
    public static class CLRConfigImpl
    {
        public static bool GetBoolValue(string switchName, out bool exist)
        {
            exist = false;
            return false;
        }
    }
}
