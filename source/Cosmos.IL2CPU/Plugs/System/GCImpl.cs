using System;

namespace Cosmos.IL2CPU.Plugs.System
{
    [Plug(Target=typeof(GC))]
    public static class GCImpl
    {
        public static void _SuppressFinalize(object o) { 
        // not implemented yet
        }
    }
}