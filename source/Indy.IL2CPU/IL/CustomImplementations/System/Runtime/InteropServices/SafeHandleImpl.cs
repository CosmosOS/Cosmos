using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.Runtime.InteropServices;

namespace Indy.IL2CPU.IL.CustomImplementations.System.Runtime.InteropServices
{
    [Plug(Target=typeof(SafeHandle))]
    public static class SafeHandleImpl
    {
        public static void InternalDispose(object aThis) { }
        public static void InternalFinalize(object aThis) { }
    }
}