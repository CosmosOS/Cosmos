using System;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U4)]
	public class Conv_U4: ILOp
	{
		public Conv_U4(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(uint aMethodUID, ILOpCode aOpCode) {
      throw new Exception("TODO:");
    }

    #region Old code
		// using System;
		// 
		// using CPUx86 = Indy.IL2CPU.Assembler.X86;
		// using Indy.IL2CPU.Assembler;
		// 
		// namespace Indy.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Conv_U4)]
		// 	public class Conv_U4: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Conv_U4(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			// todo: WARNING: not implemented correctly!
		// 			var xStackItem = Assembler.StackContents.Pop();
		// 			if (xStackItem.IsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U4: Floats not yet supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		// 				return;
		// 			}
		// 			switch (xStackItem.Size) {
		// 				case 1:
		// 				case 2: {
		// 						new CPUx86.Noop();
		// 						break;
		// 					}
		// 				case 8: {
		// 						new CPUx86.Pop{DestinationReg = CPUx86.Registers.EAX};
		//                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
		//                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 						break;
		// 					}
		// 				case 4: {
		// 						new CPUx86.Noop();
		// 						break;
		// 					}
		// 				default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		// 			}
		// 			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		// 		}
		// 	}
		// }
		#endregion Old code
	}
}
