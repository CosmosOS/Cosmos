using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldind_U1)]
	public class Ldind_U1: Op {
		public Ldind_U1(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.Move(CPUx86.Registers.EAX, "0");
			new CPUx86.Move(CPUx86.Registers.AL, CPUx86.Registers.AtECX);
			new CPUx86.Push(CPUx86.Registers.EAX);
			Assembler.StackSizes.Push(1);
		}
	}
}