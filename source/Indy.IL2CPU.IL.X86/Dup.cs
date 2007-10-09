using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Dup)]
	public class Dup: Op {
		public Dup(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			for (int i = 0; i < (Assembler.StackSizes.Peek() / 4);i++ ) {
				Move(Assembler, "eax", "[esp + " + (Assembler.StackSizes.Peek() - 4) + "]");
				Pushd("eax");
			}
			Assembler.StackSizes.Push(Assembler.StackSizes.Peek());
		}
	}
}