using System;
using Indy.IL2CPU.Plugs;

using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using HW = Cosmos.Hardware;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetEndOfKernel: AssemblerMethod {
		public override void Assemble(Assembler aAssembler) {
            new CPUx86.Push { DestinationRef = CPUAll.ElementReference.New("_end_code") };
		}

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CosCPUx86.Push { DestinationRef = CosCPUAll.ElementReference.New("_end_code") };
    }
	}
}