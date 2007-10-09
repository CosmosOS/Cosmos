using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shr)]
	public class Shr: Op {
		public Shr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax"); // shift amount
			Pop("edx"); // value
			Move(Assembler, "cl", "al");
			Move(Assembler, "ebx", "0");
			Assembler.Add(new CPU.ShiftRight("edx", "ebx", "cl"));
			Pushd("edx");
			Assembler.StackSizes.Pop();
		}
	}
}