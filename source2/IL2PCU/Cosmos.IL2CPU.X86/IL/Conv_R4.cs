using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_R4)]
	public class Conv_R4: ILOp
	{
		public Conv_R4(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// using System.IO;
		// 
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Conv_R4)]
		// 	public class Conv_R4: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Conv_R4(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		//             EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_R4: Floats not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//             return;
		// 			//int xSource = Assembler.StackContents.Pop();
		// 			//switch (xSource) {
		// 			//    case 1:
		// 			//    case 2: {
		// 			//            break;
		// 			//        }
		// 			//    case 8: {
		// 			//            new CPUx86.Pop(CPUx86.Registers_Old.EAX);
		// 			//            new CPUx86.Pop(CPUx86.Registers_Old.ECX);
		// 			//            new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
		// 			//            Assembler.StackContents.Push(4);
		// 			//            break;
		// 			//        }
		// 			//    case 4: {
		// 			//            break;
		// 			//        }
		// 			//    default:
		// 			//        throw new Exception("SourceSize " + xSource + " not supported!");
		// 			//}
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
