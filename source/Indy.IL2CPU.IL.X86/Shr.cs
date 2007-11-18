using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Shr)]
	public class Shr: Op {
		public Shr(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX); // shift amount
			int xSize = Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.EDX); // value
			xSize = Math.Max(xSize, Assembler.StackSizes.Pop());
			new CPUx86.Move(CPUx86.Registers.CL, CPUx86.Registers.AL);
			new CPUx86.Move(CPUx86.Registers.EBX, "0");
			new CPUx86.ShiftRight(CPUx86.Registers.EDX, CPUx86.Registers.EBX, CPUx86.Registers.CL);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
			Assembler.StackSizes.Push(xSize);
		}
	}
}