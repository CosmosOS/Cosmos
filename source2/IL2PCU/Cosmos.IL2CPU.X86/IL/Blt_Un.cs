using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Blt_Un)]
	public class Blt_Un: ILOp
	{
		public Blt_Un(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPU = Indy.IL2CPU.Assembler;
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Blt_Un)]
		// 	public class Blt_Un: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Blt_Un(ILReader aReader, MethodInformation aMethodInfo)
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
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Blt_Un: Floats are not yet supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			var rightTop = Assembler.StackContents.Pop();
		// 			var leftBottom = Assembler.StackContents.Pop();
		// 			int xSize = Math.Max(rightTop.Size, leftBottom.Size);
		// 			if (xSize > 8) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Blt_Un: StackSize>8 not supported yet", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			if (xSize <= 4)
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		// 				//if (xSize > 4)
		// 				//{
		// 				//    throw new NotImplementedException("long comprasion is not implemented");
		// 				//    new CPUx86.Add("esp", "4");
		// 				//}
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 				//if (xSize > 4)
		// 				//{
		// 				//    throw new NotImplementedException("long comprasion is not implemented");
		// 				//    new CPUx86.Add("esp", "4");
		// 				//}
		//                 new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
		//                 new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
		//                 new CPUx86.Jump { DestinationLabel = LabelFalse };
		// 				new CPU.Label(LabelTrue);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue=4 };
		// 				return;
		// 			}
		// 
		// 			if (xSize == 8)
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 				new CPUx86.Pop{DestinationReg = CPUx86.Registers.EDX};
		// 				//value2: EDX:EAX
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 //value1: ECX:EBX
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 				//result = value1 - value2
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = TargetLabel };
		// 			}
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Blt_Un: Circumstances not supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
