using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_I4_S)]
	public class Ldc_I4_S: Op {
		public override void Assemble(Instruction aInstruction) {
			base.Pushd(aInstruction.Operand == null ? "" : aInstruction.Operand.ToString());
		}
	}
}