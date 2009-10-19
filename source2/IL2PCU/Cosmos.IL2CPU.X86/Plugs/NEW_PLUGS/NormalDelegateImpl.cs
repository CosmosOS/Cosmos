using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Indy.IL2CPU.X86.Plugs.NEW_PLUGS;

namespace Cosmos.IL2CPU.X86.Plugs.NEW_PLUGS
{
    [Plug(Target=typeof(Delegate))]
    public static class NormalDelegateImpl
    {
        [PlugMethod(Assembler = typeof(GetMulticastInvokeAssembler))]
        public static IntPtr GetMulticastInvoke(Delegate aThis)
        {
            return IntPtr.Zero;
        }
    }
}