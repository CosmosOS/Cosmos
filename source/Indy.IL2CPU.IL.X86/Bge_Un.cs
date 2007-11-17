using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Bge_Un)]
	public class Bge_Un: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Bge_Un(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TargetLabel = GetInstructionLabel((Instruction)aInstruction.Operand);
			CurInstructionLabel = GetInstructionLabel(aInstruction);
		}
		public override void DoAssemble() {
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			new CPUx86.Pop("eax");
			new CPUx86.Compare("eax", "[esp]");
			new CPUx86.JumpIfGreaterOrEquals(LabelTrue);
			new CPUx86.JumpAlways(LabelFalse);
			new CPU.Label(LabelTrue);
			new CPUx86.Add("esp", "4");
			new CPUx86.JumpAlways(TargetLabel);
			new CPU.Label(LabelFalse);
			new CPUx86.Add("esp", "4");
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Pop();
		}
	}
}