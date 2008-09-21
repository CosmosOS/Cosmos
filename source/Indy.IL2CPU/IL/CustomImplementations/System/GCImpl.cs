using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(GC))]
    public static class GCImpl
    {
        public static void nativeSuppressFinalize(object o) { 
        // not implemented yet
        }
    }
}