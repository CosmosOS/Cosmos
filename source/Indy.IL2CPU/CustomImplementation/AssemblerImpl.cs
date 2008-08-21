using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation
{
    [Plug(Target=typeof(Assembler.Assembler))]
    public static class AssemblerImpl
    {
        [PlugMethod(Signature = "System_Void__Indy_IL2CPU_Assembler_Assembler__cctor__")]
        public static void CCtor() {
            //
        }
    }
}