using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs.Hardware.PC.Bus {
    [Plug(Target = typeof(Cosmos.Hardware.PC.Bus.CPUBus))]
    class CPUBus {
		[PlugMethod(MethodAssembler = typeof(Assemblers.IOWriteByte))]
		public static void WriteByte(ushort aPort, byte aData) { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWriteWord))]
        public static void WriteWord(ushort aPort, ushort aData) { }

        [PlugMethod(MethodAssembler = typeof(Assemblers.IOReadByte))]
		public static byte ReadByte(ushort aPort) {
			return 0;
		}

		[PlugMethod(MethodAssembler = typeof(Assemblers.IOReadWord))]
        public static ushort ReadWord(ushort aPort) {
            return 0;
        }
    }
}
