using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.Runtime.InteropServices;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System.Runtime.InteropServices
{
    [Plug(Target=typeof(SafeHandle))]
    public static class SafeHandleImpl
    {
        public static void InternalDispose(object aThis) { }
        public static void InternalFinalize(object aThis) { }
    }
}