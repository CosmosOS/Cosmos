using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Collections
{
    [Plug(TargetName = "System.Collections.HashHelpers")]
    public static class HashHelpersImpl
    {
        public static bool IsWellKnownEqualityComparer(object comparer)
        {
            return false;
        }
    }
}
