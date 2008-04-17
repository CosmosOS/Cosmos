using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ble_Un)]
	public class Ble_Un: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Ble_Un(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
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
			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";
			if (xSize > 4)
			{
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Pop(CPUx86.Registers.EDX);
				//value2: EDX:EAX
				new CPUx86.Pop("ebx");
				new CPUx86.Pop("ecx");
				//value1: ECX:EBX
				new CPUx86.Sub("ebx", "eax");
				new CPUx86.SubWithCarry("ecx", "edx");
				//result = value1 - value2
				new CPUx86.JumpIfBelowOrEqual(TargetLabel);
			} else
			{
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
				new CPUx86.JumpIfLessOrEqual(LabelFalse);
				new CPUx86.JumpAlways(LabelTrue);
				new CPU.Label(LabelTrue);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				new CPUx86.JumpAlways(TargetLabel);
				new CPU.Label(LabelFalse);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
			}
		}
	}
}