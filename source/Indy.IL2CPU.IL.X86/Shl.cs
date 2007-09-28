using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shl)]
	public class Shl: Op {
		public Shl(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax"); // shift amount
			Pop("edx"); // value
			Move(Assembler, "ebx", "0");
			Move(Assembler, "cl", "[eax]");
			Assembler.Add(new CPU.ShiftLeft("edx", "ebx", "cl"));
			Pushd("edx");
		}
	}
}