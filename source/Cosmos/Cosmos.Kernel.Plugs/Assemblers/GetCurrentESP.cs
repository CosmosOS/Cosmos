using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetCurrentESP: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP };
		}

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CosCPUx86.Push {
        DestinationReg = CosCPUx86.Registers.ESP
      };
    }
	}
}
