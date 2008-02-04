using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ble)]
	public class Ble: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Ble(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
			if (xSize > 8) {
				throw new Exception("StackSize>8 not supported");
			}
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