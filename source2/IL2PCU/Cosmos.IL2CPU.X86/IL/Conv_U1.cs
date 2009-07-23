using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_U1)]
	public class Conv_U1: ILOpX86
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_U1)]
		// 	public class Conv_U1: ILOpX86 {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Conv_U1(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			if (Assembler.StackContents.Peek().IsFloat) {
		// 				EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: Floats not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			int xSource = Assembler.StackContents.Pop().Size;
		// 			switch (xSource) {
		// 				case 2:
		// 				case 4: {
		//                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 						break;
		// 					}
		// 				case 8: {
		// 						new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		// 						new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
		// 						break;
		// 					}
		// 				case 1: {
		// 						new CPUx86.Noop();
		// 						break;
		// 					}
		// 				default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		// 			}
		// 			Assembler.StackContents.Push(new StackContent(1, typeof(byte)));
		// 		}
		// 	}
		// }
		#endregion
	}
}
