using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs.Hardware.PC.Bus {
    [Plug(Target = typeof(Cosmos.Hardware.PC.Bus.CPUBus))]
    class CPUBus {
		[PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite8))]
        public static void Write8(UInt16 aPort, byte aData) { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite16))]
        public static void Write16(UInt16 aPort, UInt16 aData) { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWrite32))]
        public static void Write32(UInt16 aPort, UInt32 aData) { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead8))]
        public static byte Read8(UInt16 aPort) {
			return 0;
		}

        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead16))]
        public static UInt16 Read16(UInt16 aPort) {
            return 0;
        }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IORead32))]
        public static UInt32 Read32(UInt16 aPort) {
            return 0;
        }
    }
}
