using Cosmos.IL2CPU.Plugs;
using System;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(IntPtr))]
    public static class IntPtrImpl
    {
        //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
        public static string ToString(IntPtr aThis)
        {
            return "<IntPtr>";
        }

        //}
    }
}