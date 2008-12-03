using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Bge_Un)]
	public class Bge_Un: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public Bge_Un(ILReader aReader, MethodInformation aMethodInfo)
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
				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
				//value2: EDX:EAX
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                //value1: ECX:EBX
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
				//result = value1 - value2
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = TargetLabel };
			} else {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true};
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = LabelFalse };
                new CPUx86.Jump { DestinationLabel = LabelTrue };
				new CPU.Label(LabelTrue);
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Jump { DestinationLabel = TargetLabel };
				new CPU.Label(LabelFalse);
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
			}
		}
	}
}