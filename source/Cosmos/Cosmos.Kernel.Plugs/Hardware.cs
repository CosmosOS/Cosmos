using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(HW.Hardware))]
    public static class Hardware {
        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite))]
        public static void IOWrite(ushort aPort, byte aData) { }
        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead))]
        public static byte IORead(ushort aPort) { return 0; }
    }
}
