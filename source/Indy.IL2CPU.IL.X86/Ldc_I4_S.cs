using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldc_I4_S)]
	public class Ldc_I4_S: Op {
		public readonly string Value;
		public Ldc_I4_S(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			Value = (aInstruction.Operand ?? "").ToString();
		}

		public override void Assemble() {
			Pushd(Value);
		}
	}
}