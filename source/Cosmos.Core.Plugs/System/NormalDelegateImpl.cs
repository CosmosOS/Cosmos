using System;

using Cosmos.Core.Plugs.System.Assemblers;
using Cosmos.IL2CPU.Plugs.Assemblers;

namespace Cosmos.IL2CPU.Plugs.System
{
    [Plug(Target = typeof(Delegate))]
    public static class NormalDelegateImpl
    {
        [PlugMethod(Assembler = typeof(GetMulticastInvokeAssembler))]
        public static IntPtr GetMulticastInvoke(Delegate aThis)
        {
            return IntPtr.Zero;
        }
    }
}
