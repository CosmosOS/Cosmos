using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Pop)]
	public class Pop: Op {
		public Pop(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {

		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			Assembler.StackSizes.Pop();
		}
	}
}