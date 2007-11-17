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
			new CPUx86.Pop("ecx");
			new CPUx86.Pop("eax");
			new CPUx86.Pushd("ecx");
			new CPUx86.Compare("eax", "[esp]");
			new CPUx86.JumpIfLess(LabelTrue);
			new CPUx86.JumpAlways(LabelFalse);
			new CPU.Label(LabelTrue);				
			new CPUx86.Add("esp", "4");
			new CPUx86.Push("01h");
			new CPUx86.JumpAlways(NextInstructionLabel);
			new CPU.Label(LabelFalse);
			new CPUx86.Add("esp", "4");
			new CPUx86.Push("00h");
			new CPUx86.JumpAlways(NextInstructionLabel);
		}
	}
}