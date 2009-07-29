using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bge_Un)]
	public class Bge_Un: ILOp
	{
		public Bge_Un(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
      //TODO: Implement this Op
      //TODO: Merge these like Branch if possible, ie Branch_Un
    }

    
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Bge_Un)]
		// 	public class Bge_Un: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Bge_Un(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 			CurInstructionLabel = GetInstructionLabel(aReader);
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			if (Assembler.StackContents.Peek().IsFloat) {
		//                 EmitNotSupportedException(Assembler, GetServiceProvider(), "Floats are not yet supported (Bge)", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			int xSize = Math.Max(Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size);
		// 			if (xSize > 8) {
		//                 EmitNotSupportedException(Assembler, GetServiceProvider(), "StackSize>8 not supported (Bge)", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			if (xSize > 4)
		// 			{
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		// 				//value2: EDX:EAX
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 //value1: ECX:EBX
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 				//result = value1 - value2
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = TargetLabel };
		// 			} else {
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true};
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThanOrEqualTo, DestinationLabel = LabelFalse };
		//                 new CPUx86.Jump { DestinationLabel = LabelTrue };
		// 				new CPU.Label(LabelTrue);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		// 			}
		// 		}
		// 	}
		// }
		
	}
}
