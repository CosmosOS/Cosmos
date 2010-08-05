using System;
using Cosmos.IL2CPU.Plugs;

using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetEndOfKernel: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Push { DestinationRef = CPUAll.ElementReference.New("_end_code") };
    }
	}
}