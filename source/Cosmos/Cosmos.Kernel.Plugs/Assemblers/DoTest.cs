using System;
using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class DoTest: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Call {
        DestinationLabel = "_CODE_REQUESTED_BREAK_"
      };
      new Label("DO_THE_TEST");
      new CPUx86.Interrupt {
        DestinationValue = 0x35
      };
    }
	}
}
