using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ble)]
	public class Ble: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Ble(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			TargetLabel = GetInstructionLabel((Instruction)aInstruction.Operand);
			CurInstructionLabel = GetInstructionLabel(aInstruction);
		}
		public override void DoAssemble() {
			int xSize = Math.Max(Assembler.StackSizes.Pop(), Assembler.StackSizes.Pop());
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			new CPUx86.Pop(CPUx86.Registers.EAX);
			if (xSize > 4) {
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
			}
			new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			new CPUx86.JumpIfLessOrEqual(LabelFalse);
			new CPUx86.JumpAlways(LabelTrue);
			new CPU.Label(LabelTrue);
			if (xSize > 4) {
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
			}
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			new CPUx86.JumpAlways(TargetLabel);
			new CPU.Label(LabelFalse);
			if (xSize > 4) {
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
			}
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
		}
	}
}