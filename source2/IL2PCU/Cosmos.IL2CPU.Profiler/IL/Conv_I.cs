using System;

namespace Cosmos.IL2CPU.Profiler.IL
{
	[Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_I)]
	public class Conv_I: ILOpProfiler
	{



		#region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[Cosmos.IL2CPU.OpCode(ILOp.Code.Conv_I)]
		// 	public class Conv_I: ILOpProfiler {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Conv_I(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			var xSource = Assembler.StackContents.Pop();
		// 			switch (xSource.Size) {
		// 				case 1:
		// 				case 2:
		// 				case 4: {
		// 						new CPUx86.Noop();
		// 						break;
		// 					}
		// 				case 8: {
		// //    					new CPUx86.Pop(CPUx86.Registers_Old.EAX);
		//                         new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
		// //						new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
		// 						break;
		// 
		// 					}
		// 				default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		// 			}
		// 			Assembler.StackContents.Push(new StackContent(4, true, false, false));
		// 		}
		// 	}
		// }
		#endregion
	}
}
