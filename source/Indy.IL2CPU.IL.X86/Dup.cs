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
			int xTheSize = Assembler.StackSizes.Peek();
			for (int i = 0; i < (xTheSize / 4); i++) {
				new CPU.Move("eax", "[esp + " + (i * 4) + "]");
				new CPU.Pushd("eax");
			}
			Assembler.StackSizes.Push(xTheSize);
		}
	}
}