using Cosmos.IL2CPU.Plugs;
using System;
using System.Collections;

namespace Cosmos.System.Plugs.System.Collections
{
    [Plug(TargetName = "System.Collections.HashHelpers")]
    public static class HashHelpersImpl
    {
        public static bool IsWellKnownEqualityComparer(object comparer)
        {
            return false;
        }

        public static IEqualityComparer GetRandomizedEqualityComparer(object comparer)
        {
            throw new NotImplementedException();
        }
    }
}
