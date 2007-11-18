using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Div)]
	public class Div: Op {
		public Div(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.ECX);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Divide(CPUx86.Registers.ECX);
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			Assembler.StackSizes.Pop();
		}
	}
}