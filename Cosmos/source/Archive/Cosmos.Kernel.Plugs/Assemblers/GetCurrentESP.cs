using System;
using System.Collections.Generic;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetCurrentESP: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Push(XSRegisters.ESP);
    }
	}
}
