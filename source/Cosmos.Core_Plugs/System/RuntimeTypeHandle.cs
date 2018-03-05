using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(global::System.RuntimeTypeHandle))]
    public static class RuntimeTypeHandleImpl
    {
        public static unsafe void Ctor(RuntimeTypeHandle aThis, void* aValue)
        {
        }

        public static void Cctor()
        {
            //
        }
    }
}
