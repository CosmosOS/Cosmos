using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Div_Un)]
	public class Div_Un: Op {
		public Div_Un(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			int xSize = Assembler.StackSizes.Pop();
			if (xSize == 8) {
				//TODO: implement proper div support for 8byte values!
				new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);
				new CPUx86.Pop(CPUx86.Registers.ECX);
				new CPUx86.Add("esp", "4");
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Add("esp", "4");
				new CPUx86.Divide(CPUx86.Registers.ECX);
				new CPUx86.Push("0");
				new CPUx86.Pushd(CPUx86.Registers.EAX);

			} else {
				new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);
				new CPUx86.Pop(CPUx86.Registers.ECX);
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Divide(CPUx86.Registers.ECX);
				new CPUx86.Pushd(CPUx86.Registers.EAX);
			}
		}
	}
}