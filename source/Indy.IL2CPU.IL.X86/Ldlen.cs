using System;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldlen, true)]
	public class Ldlen: Op {
		public Ldlen(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}
		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Add(CPUx86.Registers.EAX, "8");
			new CPUx86.Pushd(CPUx86.Registers.EAX);
			Assembler.StackSizes.Push(4);
		}
	}
}