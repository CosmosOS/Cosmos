using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Sub)]
	public class Sub: ILOpX86
	{
		public Sub(ILOpCode aOpCode):base(aOpCode)
		{
		}

		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Sub)]
		// 	public class Sub: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Sub(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			var stackTop = Assembler.StackContents.Pop();
		// 
		//             if (stackTop.IsFloat)
		//             {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Sub: Float support not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		//             }
		// 
		// 			switch (stackTop.Size) {
		// 				case 1:
		// 				case 2:
		// 				case 4:
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX }; 
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX };
		//                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 					break;
		// 				case 8:
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
		//                     new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
		//                     new CPUx86.SubWithCarry { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
		// 					break;
		// 				default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Sub: Size not supported: " + stackTop.Size, mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		// 			}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
