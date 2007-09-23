using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Bgt)]
	public class Bgt: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Bgt(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TargetLabel = GetInstructionLabel((Instruction)aInstruction.Operand);
			CurInstructionLabel = GetInstructionLabel(aInstruction);
		}
		public override void DoAssemble() {
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			Assembler.Add(new CPUx86.Pop("eax"));
			Assembler.Add(new CPUx86.Compare("eax", "[esp]"));
			Assembler.Add(new CPUx86.JumpIfGreater(LabelTrue));
			Assembler.Add(new CPUx86.JumpAlways(LabelFalse));
			Assembler.Add(new CPU.Label(LabelTrue));
			Assembler.Add(new CPUx86.Add("esp", "4"));
			Assembler.Add(new CPUx86.JumpAlways(TargetLabel));
			Assembler.Add(new CPU.Label(LabelFalse));
			Assembler.Add(new CPUx86.Add("esp", "4"));
		}
	}
}