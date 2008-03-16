using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Bge)]
	public class Bge: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Bge(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
		}

		private void DoAssemble32Bit() {
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
#warning Code checking: strange code seems to be generated. Read the following comments:
			//JumpAlways right after JumpIfGreaterOrEquals to the same label
			//my offer is:
			new CPUx86.Call("_CODE_REQUESTED_BREAK_");
			new CPUx86.Pop(CPUx86.Registers.EBX);
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.EBX);
			new CPUx86.JumpIfGreaterOrEquals(LabelTrue);
			new CPUx86.JumpAlways(LabelFalse);
			new CPU.Label(LabelTrue);
			new CPUx86.JumpAlways(TargetLabel);
			new CPU.Label(LabelFalse);
			//new CPUx86.Pop(CPUx86.Registers.EAX);
			//new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
			//new CPUx86.JumpIfGreaterOrEquals(LabelTrue);
			//new CPUx86.JumpAlways(LabelTrue);
			//new CPU.Label(LabelTrue);
			//new CPUx86.Add(CPUx86.Registers.ESP, "4");
			//new CPUx86.JumpAlways(TargetLabel);
			//new CPU.Label(LabelFalse);
			//new CPUx86.Add(CPUx86.Registers.ESP, "4");
		}

		private void DoAssemble64Bit() {
			throw new Exception("Not implemented");
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			new CPUx86.Pop(CPUx86.Registers.EAX);
			new CPUx86.Compare(CPUx86.Registers.EAX, "[esp + 4]");
			new CPUx86.JumpIfGreaterOrEquals(LabelFalse);
			new CPUx86.JumpAlways(LabelTrue);
			new CPU.Label(LabelTrue);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
			new CPUx86.JumpAlways(TargetLabel);
			new CPU.Label(LabelFalse);
			new CPUx86.Add(CPUx86.Registers.ESP, "4");
		}

		public override void DoAssemble() {
			StackContent xItem1 = Assembler.StackContents.Pop();
			StackContent xItem2 = Assembler.StackContents.Pop();
			int xSize = Math.Max(xItem1.Size, xItem2.Size);
			var xIsFloat = xItem1.IsFloat || xItem2.IsFloat;
			if (xIsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			if (xSize > 8) {
				throw new Exception("StackSize>8 not supported");
			}
			if (xSize > 4) {
				DoAssemble64Bit();
			} else {
				DoAssemble32Bit();
			}
		}
	}
}
