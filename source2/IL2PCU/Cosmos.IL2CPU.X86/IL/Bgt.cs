using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Bgt)]
	public class Bgt: ILOp
	{
		public Bgt(ILOpCode aOpCode):base(aOpCode)
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
		// 	[OpCode(OpCodeEnum.Bgt)]
		// 	public class Bgt: Op {
		// 		public readonly string TargetLabel;
		// 		public readonly string CurInstructionLabel;
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Bgt(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		// 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
		// 			CurInstructionLabel = GetInstructionLabel(aReader);
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			string BaseLabel = CurInstructionLabel + "__";
		// 			string LabelTrue = BaseLabel + "True";
		// 			string LabelFalse = BaseLabel + "False";
		// 			var xStackContent = Assembler.StackContents.Pop();
		// 			if (xStackContent.IsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Bgt: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			Assembler.StackContents.Pop();
		// 			if (xStackContent.Size > 8) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Bgt: StackSize>8 not supported yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			if (xStackContent.Size > 4)
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		// 				//value2: EDX:EAX
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                 //value1: ECX:EBX
		//                 new CPUx86.Sub { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EAX };
		//                 new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EDX };
		// 				//result = value1 - value2
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.GreaterThan, DestinationLabel = TargetLabel };
		// 			}else
		// 			{
		//                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		// 				new CPUx86.Compare{DestinationReg=CPUx86.Registers.EAX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true};
		//                 new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = LabelTrue };
		//                 new CPUx86.Jump { DestinationLabel = LabelFalse };
		// 				new CPU.Label(LabelTrue);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		//                 new CPUx86.Jump { DestinationLabel = TargetLabel };
		// 				new CPU.Label(LabelFalse);
		//                 new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		// 			}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
