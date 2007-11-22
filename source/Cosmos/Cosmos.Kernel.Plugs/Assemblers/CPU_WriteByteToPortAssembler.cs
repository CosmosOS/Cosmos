using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public sealed class CPU_WriteByteToPortAssembler: BaseMethodAssembler {
		public override void Assemble(Assembler aAssembler) {
			new CPUx86.Move(Registers.EDX, "[ebp + 0xC]");
			new CPUx86.Move(Registers.EAX, "[ebp + 0x8]");
			new CPUNative.Out(Registers.DX, Registers.AL);
		}
	}
}