using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using HW = Cosmos.Hardware2;

namespace Cosmos.Kernel.Plugs.Other {
    [Plug(Target = typeof(HW.Hardware))]
    public static class Hardware {
        [PlugMethod(Assembler = typeof(Assemblers.IOWrite8))]
        public static void IOWriteByte(ushort aPort, byte aData) {
        }
        [PlugMethod(Assembler = typeof(Assemblers.IORead8))]
        public static byte IOReadByte(ushort aPort) {
            return 0;
        }
        [PlugMethod(Assembler = typeof(Assemblers.IOWrite16))]
        public static void IOWriteWord(ushort aPort, ushort aData) {
        }
        [PlugMethod(Assembler = typeof(Assemblers.IORead16))]
        public static ushort IOReadWord(ushort aPort) {
            return 0;
        }

    }
}
