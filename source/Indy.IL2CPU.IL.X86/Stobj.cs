using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stobj)]
	public class Stobj: Op {
		public Stobj(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Pop("eax");
			Pop("edx");
			Move(Assembler, "[eax]", "edx");
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Pop();
		}
	}
}