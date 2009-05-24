using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetEndOfStack: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new CPUx86.Push { DestinationRef = ElementReference.New("Kernel_Stack") };
		}
	}
}
