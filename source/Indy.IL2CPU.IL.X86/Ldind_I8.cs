using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldind_I8)]
	public class Ldind_I8: Op {
		public Ldind_I8(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Pushd("[eax + 4]");
			new CPUx86.Pushd(CPUx86.Registers.AtEAX);
			Assembler.StackSizes.Push(8);
		}
	}
}