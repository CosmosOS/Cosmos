using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs.Hardware.PC.Bus {
    [Plug(Target = typeof(Cosmos.Kernel.CPUBus))]
    class CPUBus {
		[PlugMethod(Assembler = typeof(Assemblers.IOWrite8))]
        public static void Write8(UInt16 aPort, byte aData) { }

        [PlugMethod(Assembler = typeof(Assemblers.IOWrite16))]
        public static void Write16(UInt16 aPort, UInt16 aData) { }

        [PlugMethod(Assembler = typeof(Assemblers.IOWrite32))]
        public static void Write32(UInt16 aPort, UInt32 aData) { }

        [PlugMethod(Assembler = typeof(Assemblers.IORead8))]
        public static byte Read8(UInt16 aPort) {
			return 0;
		}

        [PlugMethod(Assembler = typeof(Assemblers.IORead16))]
        public static UInt16 Read16(UInt16 aPort) {
            return 0;
        }

        [PlugMethod(Assembler = typeof(Assemblers.IORead32))]
        public static UInt32 Read32(UInt16 aPort) {
            return 0;
        }
    }
}
