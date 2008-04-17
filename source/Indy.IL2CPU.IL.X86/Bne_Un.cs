using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Bne_Un)]
	public class Bne_Un: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Bne_Un(ILReader aReader, MethodInformation aMethodInfo)
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
			if (xSize > 4)
			{
				new CPUx86.Pop("eax");
				new CPUx86.Pop("ebx");
				//value2 = EBX:EAX
				new CPUx86.Pop("ecx");
				new CPUx86.Pop("edx");
				//value1 = EDX:ECX
				new CPUx86.Xor("eax", "ecx");
				new CPUx86.JumpIfNotZero(TargetLabel);
				new CPUx86.Xor("ebx", "edx");
				new CPUx86.JumpIfNotZero(TargetLabel);
			} else
			{
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Compare(CPUx86.Registers.EAX, CPUx86.Registers.AtESP);
				new CPUx86.JumpIfEquals(LabelTrue);
				new CPUx86.JumpAlways(LabelFalse);
				new CPU.Label(LabelFalse);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
				new CPUx86.JumpAlways(TargetLabel);
				new CPU.Label(LabelTrue);
				new CPUx86.Add(CPUx86.Registers.ESP, "4");
			}
		}
	}
}