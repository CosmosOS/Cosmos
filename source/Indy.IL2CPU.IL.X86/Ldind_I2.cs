using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldind_I2)]
	public class Ldind_I2: Op {
		public Ldind_I2(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Move(CPUx86.Registers.EDX, "0");
			new CPUx86.Move(CPUx86.Registers.DX, CPUx86.Registers.AtEAX);
			new CPUx86.Pushd(CPUx86.Registers.EDX);
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Push(2);
		}
	}
}