using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U8)]
	public class Conv_U8: ILOp
	{
		public Conv_U8(Cosmos.IL2CPU.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {
        var xSource = Assembler.Stack.Pop();
        switch( xSource.Size )
        {
            case 1:
            case 2:
            case 4:
                {
                    if (xSource.IsFloat)
                    {
                        new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 32, DestinationIsIndirect = true };
                        new CPUx86.x87.FloatABS();
                        new CPUx86.x87.FloatRound();
                        new CPUx86.x87.FloatStore { DestinationReg = Registers.ESP, Size = 32, DestinationIsIndirect = true };
                    }
                    else
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationValue = 0 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    }
                    break;
                }
            case 8:
                {
                    if (xSource.IsFloat)
                    {
                        new CPUx86.x87.FloatLoad { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                        new CPUx86.x87.FloatABS();
                        new CPUx86.x87.FloatRound();
                        new CPUx86.x87.FloatStore { DestinationReg = Registers.ESP, Size = 64, DestinationIsIndirect = true };
                    }
                    break;
                }
            default:
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                throw new NotImplementedException();
        }
        Assembler.Stack.Push(8, typeof(ulong));
    }

    
		// using System;
		// 
		// using CPUx86 = Cosmos.IL2CPU.X86;
		// using Cosmos.IL2CPU.X86;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Conv_U8)]
		// 	public class Conv_U8: Op {
		//         private string mNextLabel;
		// 	    private string mCurLabel;
		// 	    private uint mCurOffset;
		// 	    private MethodInformation mMethodInformation;
		// 		public Conv_U8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo) {
		//              mMethodInformation = aMethodInfo;
		// 		    mCurOffset = aReader.Position;
		// 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
		//             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		// 		}
		// 		public override void DoAssemble() {
		// 			if (Assembler.Stack.Peek().IsFloat) {
		//                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U8: Floats are not yet supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                 return;
		// 			}
		// 			int xSource = Assembler.Stack.Peek().Size;
		// 			switch (xSource) {
		// 				case 1:
		// 				case 2:
		// 				case 4: {
		// 						Assembler.Stack.Pop();
		//                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
		//                         new CPUx86.Push { DestinationValue = 0 };
		//                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		// 						Assembler.Stack.Push(new StackContent(8, typeof(ulong)));
		// 						break;
		// 					}
		// 				case 8: {
		// 						new CPUx86.Noop();
		// 						break;
		// 					}
		// 				default:
		//                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		//                     return;
		// 			}
		// 		}
		// 	}
		// }
		
	}
}
