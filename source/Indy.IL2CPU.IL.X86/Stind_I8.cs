using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stind_I8)]
	public class Stind_I8: Op {
		public Stind_I8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax");
			Pop("ecx");
			Pop("edx");
			Move(Assembler, "[edx]", "eax");
			Move(Assembler, "[edx + 4]", "ecx");
		}
	}
}