using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPU = Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Clt)]
	public class Clt: Op {
		private readonly string NextInstructionLabel;
		private readonly string CurInstructionLabel;
		public Clt(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			NextInstructionLabel = GetInstructionLabel(aInstruction.Next);
			CurInstructionLabel = GetInstructionLabel(aInstruction);
		}
		public override void DoAssemble() {
			Assembler.StackSizes.Pop();
			Assembler.StackSizes.Push(4);
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			Assembler.Add(new CPUx86.Pop("ecx"));
			Assembler.Add(new CPUx86.Pop("eax"));
			Pushd("ecx");
			Assembler.Add(new CPUx86.Compare("eax", "[esp]"));
			Assembler.Add(new CPUx86.JumpIfLess(LabelTrue));
			Assembler.Add(new CPUx86.JumpAlways(LabelFalse));
			Assembler.Add(new CPU.Label(LabelTrue));				
			Assembler.Add(new CPUx86.Add("esp", "4"));
			Assembler.Add(new CPUx86.Push("01h"));
			Assembler.Add(new CPUx86.JumpAlways(NextInstructionLabel));
			Assembler.Add(new CPU.Label(LabelFalse));
			Assembler.Add(new CPUx86.Add("esp", "4"));
			Assembler.Add(new CPUx86.Push("00h"));
			Assembler.Add(new CPUx86.JumpAlways(NextInstructionLabel));
		}
	}
}