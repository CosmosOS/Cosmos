using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(GC))]
    public static class GCImpl
    {
        public static void _SuppressFinalize(object o) { 
        // not implemented yet
        }
    }
}