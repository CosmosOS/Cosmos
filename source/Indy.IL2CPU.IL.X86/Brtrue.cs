using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Brtrue)]
	public class Brtrue: Op {
		public readonly string TargetLabel;
		public Brtrue(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TargetLabel = GetInstructionLabel((Instruction)aInstruction.Operand);
		}
		public override void DoAssemble() {
			new CPUx86.Popd(CPUx86.Registers.EAX);
			new CPUx86.Compare(CPUx86.Registers.EAX, "0");
			new CPUx86.JumpIfNotEquals(TargetLabel);
			Assembler.StackSizes.Pop();
		}
	}
}