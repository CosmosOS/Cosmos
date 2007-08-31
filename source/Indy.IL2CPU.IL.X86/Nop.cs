using System;
using System.IO;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class Nop: IL.Nop {
		public override void Assemble(Instruction aInstruction) {
			new CPU.Noop();
		}
	}
}