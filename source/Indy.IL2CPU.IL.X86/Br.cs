using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Br)]
	public class Br: Op {
		private readonly string mTargetInstructionName;
		public Br(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			mTargetInstructionName = GetInstructionLabel((Instruction)aInstruction.Operand);
		}
		public override void DoAssemble() {
			Assembler.Add(new CPU.JumpAlways(mTargetInstructionName));
		}
	}
}