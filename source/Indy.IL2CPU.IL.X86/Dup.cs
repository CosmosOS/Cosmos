using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Dup)]
	public class Dup: Op {
		public Dup(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xTheSize = Assembler.StackSizes.Peek();
			for (int i = 0; i < (xTheSize / 4); i++) {
				new CPUx86.Move(CPUx86.Registers.EAX, "[esp " + (i > 0 ? "+" + (i * 4) : "") + "]");
				new CPUx86.Pushd(CPUx86.Registers.EAX);
			}
			Assembler.StackSizes.Push(xTheSize);
		}
	}
}