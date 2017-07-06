using System;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target=typeof(GC))]
    public static class GCImpl
    {
        public static void _SuppressFinalize(object o) {
        // not implemented yet
        }
    }
}
