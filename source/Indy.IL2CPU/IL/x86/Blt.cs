using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Blt)]
	public class Blt: Op {
		public readonly string TargetLabel;
		public readonly string CurInstructionLabel;
		public readonly string NextInstructionLabel;
		public Blt(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
			CurInstructionLabel = GetInstructionLabel(aReader);
			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
		}
		public override void DoAssemble() {
			if (Assembler.StackContents.Peek().IsFloat) {
				throw new Exception("Floats not yet supported!");
			}
			var right = Assembler.StackContents.Pop();
			var left = Assembler.StackContents.Pop();
            if (right.Size != left.Size)
            {
                if (right.Size > 4 || left.Size > 4)
                {
                    throw new NotImplementedException("mixed size operations are not implemented! (" + left.Size + "/" + right.Size + ")");
                }
            }

			int xSize = right.Size;

			if (xSize > 8)
				throw new NotImplementedException("StackSize>8 not supported");

			string BaseLabel = CurInstructionLabel + "__";
			string LabelTrue = BaseLabel + "True";
			string LabelFalse = BaseLabel + "False";

			switch(xSize)
			{
			case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
                new CPUx86.Jump { DestinationLabel = LabelFalse };
				new CPU.Label(LabelTrue);
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                new CPUx86.Jump { DestinationLabel = TargetLabel };
				new CPU.Label(LabelFalse);
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
				break;
			case 8:
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                //value2: EDX:EAX
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                //value1: ECX:EBX
                new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
                new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
				//result = value1 - value2
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = TargetLabel };
				break;
			default:
				throw new NotSupportedException(string.Format("comprasion of {0} byte values", xSize));
			}
		}
	}
}