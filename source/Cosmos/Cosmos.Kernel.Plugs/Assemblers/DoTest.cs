using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class DoTest: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new CPUx86.Call { DestinationLabel = "_CODE_REQUESTED_BREAK_" };
			new Label("DO_THE_TEST");
            new CPUx86.Interrupt { DestinationValue = 0x35 };
		}
	}
}
