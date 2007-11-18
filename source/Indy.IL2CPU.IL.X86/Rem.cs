using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Rem)]
	public class Rem: Op {
		public Rem(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackSizes.Pop(), Assembler.StackSizes.Pop());
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.Pop(CPUx86.Registers.EAX); // gets devised by ecx
			new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);
			new CPUx86.Divide(CPUx86.Registers.ECX);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
			Assembler.StackSizes.Push(xSize);
		}
	}
}