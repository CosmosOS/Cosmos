using System;
using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(global::System.RuntimeTypeHandle), IsMicrosoftdotNETOnly = true)]
    public static class RuntimeTypeHandleImpl
    {
        //[PlugMethod(Signature = "System_Void__System_RuntimeTypeHandle__ctor_System_Void__")]
        public static unsafe void Ctor(RuntimeTypeHandle aThis, void* aValue)
        {
        }

        public static void Cctor() {
          //
        }
    }
}
