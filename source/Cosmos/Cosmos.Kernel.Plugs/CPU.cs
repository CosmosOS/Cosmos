using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(Hardware.CPU))]
	public static class CPU {
		[PlugMethod(MethodAssembler = typeof(Assemblers.CPU_WriteByteToPortAssembler))]
		public static void WriteByteToPort(ushort aPort, byte aData) {
			//			Assemblers.CPU_WriteByteToPortAssembler x = new Cosmos.Kernel.Plugs.Assemblers.CPU_WriteByteToPortAssembler();
			//			FakeMethod(x);
		}

		[PlugMethod(MethodAssembler=typeof(Assemblers.CPU_EnableSimpleGDTAssembler))]
		public static void EnableSimpleGDT() {
		}

		//		public static void FakeMethod(Assemblers.CPU_WriteByteToPortAssembler aAssembler) {
		//			aAssembler.Assemble(null);
		//		}
	}
}
