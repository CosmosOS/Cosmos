using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs {
    [Plug(Target = typeof(HW.Hardware))]
    public static class Hardware {
        [PlugMethod(MethodAssembler = typeof(Assemblers.IOWriteByte))]
        public static void IOWriteByte(ushort aPort, byte aData) { }
        [PlugMethod(MethodAssembler = typeof(Assemblers.IOReadByte))]
        public static byte IOReadByte(ushort aPort) { return 0; }
		[PlugMethod(MethodAssembler = typeof(Assemblers.IOWriteWord))]
		public static void IOWriteWord(ushort aPort, ushort aData) {
		}
		[PlugMethod(MethodAssembler = typeof(Assemblers.IOReadWord))]
		public static ushort IOReadWord(ushort aPort) {
			return 0;
		}
    }
}
