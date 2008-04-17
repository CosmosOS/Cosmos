using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Beq)]
	public class Beq: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Beq(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}
		public override void DoAssemble() {
			var xStackContent = Assembler.StackContents.Pop();
			Assembler.StackContents.Pop();
			if (xStackContent.Size > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			if (xStackContent.Size <= 4)
			{
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Pop(CPUx86.Registers.EBX);
				new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.EBX);
				new CPUx86.JumpIfNotEquals(LabelFalse);
				new CPUx86.JumpAlways(TargetLabel);
				new CPU.Label(LabelFalse);
				//new CPUx86.Noop();
				//new CPUx86.JumpAlways(LabelFalse);
				//new CPU.Label(LabelTrue);
				//new CPUx86.Add(CPUx86.Registers.ESP, "4");
				//new CPUx86.JumpAlways(TargetLabel);
				//new CPU.Label(LabelFalse);
				//new CPUx86.Add(CPUx86.Registers.ESP, "4");
			} else
			{
				new CPUx86.Pop("eax");
				new CPUx86.Pop("ebx");
				new CPUx86.Pop("ecx");
				new CPUx86.Pop("edx");
				new CPUx86.Xor("eax", "ecx");
				new CPUx86.JumpIfNotZero(LabelFalse);
				new CPUx86.Xor("ebx", "edx");
				new CPUx86.JumpIfNotZero(LabelFalse);
				new CPUx86.JumpAlways(TargetLabel);
				new CPU.Label(LabelFalse);
				//new CPUx86.Noop();
			}
		}
	}
}