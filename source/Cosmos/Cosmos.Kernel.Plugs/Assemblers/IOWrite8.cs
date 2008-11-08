using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public sealed class IOWrite8: AssemblerMethod {
		public override void Assemble(Assembler aAssembler) {
            //TODO: This is a lot of work to write to a single port. We need to have some kind of inline ASM option that can emit a single out instruction
            new CPUx86.Move { DestinationReg = Registers.EDX, SourceReg = Registers.EBP, SourceDisplacement = 0xC, SourceIsIndirect = true };
            new CPUx86.Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBP, SourceDisplacement = 0x8, SourceIsIndirect = true };
            new Out { Size = 8 };
		}
	}
}