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
			new CPUx86.Pop(CPUx86.Registers.EAX);
			if (xStackContent.Size > 4) {
				throw new NotImplementedException("long comprasion is not implemented");
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			new CPUx86.JumpIfEquals(LabelTrue);
			new CPUx86.JumpAlways(LabelFalse);
			new CPU.Label(LabelTrue);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			if (xStackContent.Size > 4) {
				throw new NotImplementedException("long comprasion is not implemented");
				new CPUx86.Add("esp", "4");
			}
			new CPUx86.JumpAlways(TargetLabel);
			new CPU.Label(LabelFalse);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			if (xStackContent.Size > 4) {
				throw new NotImplementedException("long comprasion is not implemented");
				new CPUx86.Add("esp", "4");
			}
		}
	}
}