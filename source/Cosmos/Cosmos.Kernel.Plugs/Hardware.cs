using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs.Other {
    [Plug(Target = typeof(HW.Hardware))]
    public static class Hardware {
        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite8))]
        public static void IOWriteByte(ushort aPort, byte aData) {
        }
        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead8))]
        public static byte IOReadByte(ushort aPort) {
            return 0;
        }
        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite16))]
        public static void IOWriteWord(ushort aPort, ushort aData) {
        }
        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead16))]
        public static ushort IOReadWord(ushort aPort) {
            return 0;
        }

    }
}
