using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge)]
	public class Bge: ILOp
	{
		public Bge(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      //TODO: Implement this Op
    }

		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Bge)]
		// 	public class Bge: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Bge(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 			CurInstructionLabel = GetInstructionLabel(aReader);
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 
		// 		private void DoAssemble32Bit() {
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// #warning Code checking: strange code seems to be generated. Read the following comments:
		// 			//Jump right after JumpIfGreaterOrEquals to the same label
		// 			//my offer is:
		// 			new CPUx86.Pop{DestinationReg = CPUx86.Registers.EBX};
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg=CPUx86.Registers.EBX };
		//             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = LabelTrue };
		//             new CPUx86.Jump { DestinationLabel = LabelFalse };
		// 			new CPU.Label(LabelTrue);
		//             new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 			new CPU.Label(LabelFalse);
		// 			//new CPUx86.Pop(CPUx86.Registers_Old.EAX);
		// 			//new CPUx86.Compare(CPUx86.Registers_Old.EAX, CPUx86.Registers_Old.AtESP);
		// 			//new CPUx86.JumpIfGreaterOrEquals(LabelTrue);
		// 			//new CPUx86.Jump(LabelTrue);
		// 			//new CPU.Label(LabelTrue);
		// 			//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
		// 			//new CPUx86.Jump(TargetLabel);
		// 			//new CPU.Label(LabelFalse);
		// 			//new CPUx86.Add(CPUx86.Registers_Old.ESP, "4");
		// 		}
		// 
		// 		private void DoAssemble64Bit() {
		// 			//throw new NotImplementedException("long comprasion is not implemented");
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		// 			//value2: EDX:EAX
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//             new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		// 			//value1: ECX:EBX
		//             new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//             new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 			//result = value1 - value2
		//             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = TargetLabel };
		// 		}
		// 
		// 		public override void DoAssemble() {
		// 			StackContent xItem1 = Assembler.StackContents.Pop();
		// 			StackContent xItem2 = Assembler.StackContents.Pop();
		// 			int xSize = Math.Max(xItem1.Size, xItem2.Size);
		// 			var xIsFloat = xItem1.IsFloat || xItem2.IsFloat;
		// 			if (xIsFloat) {
		//                 EmitNotSupportedException(Assembler, GetServiceProvider(), "Floats are not yet supported (Bge)", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			if (xSize > 8) {
		//                 EmitNotSupportedException(Assembler, GetServiceProvider(), "StackSize>8 not supported (Bge)", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			if (xSize > 4) {
		// 				DoAssemble64Bit();
		// 			} else {
		// 				DoAssemble32Bit();
		// 			}
		// 		}
		// 	}
    // }
  }
}
