using System;
using System.IO;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	public class Ldc_I4_1: IL.Ldc_I4_1 {
		public override void Assemble(Instruction aInstruction) {
			new CPU.Pushd("1");
		}
	}
}