using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Brfalse)]
	public class Brfalse: Op {
		public readonly string TargetLabel;
		public Brfalse(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TargetLabel = GetInstructionLabel((Instruction)aInstruction.Operand);
		}
		public override void DoAssemble() {
			Assembler.Add(new CPU.Popd("eax"));
			Assembler.Add(new CPU.Compare("eax", "00h"));
			Assembler.Add(new CPU.JumpIfEquals(TargetLabel));
			Assembler.StackSizes.Pop();
		}
	}
}