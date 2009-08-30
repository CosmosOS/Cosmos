using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U1 )]
    public class Conv_U1 : ILOp
    {
        public Conv_U1( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            if( Assembler.Stack.Peek().IsFloat )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: Floats not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                throw new NotImplementedException();
            }
            int xSource = Assembler.Stack.Pop().Size;
            switch( xSource )
            {
                case 2:
                case 4:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 8:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 1:
                    {
                        new CPUx86.Noop();
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I1: SourceSize " + xSource + " not supported", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push( 1, typeof( byte ) ) ;
        }


        // using System;
        // 
        // using CPUx86 = Indy.IL2CPU.Assembler.X86;
        // using Indy.IL2CPU.Assembler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_U1)]
        // 	public class Conv_U1: Op {
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
        // 			if (Assembler.Stack.Peek().IsFloat) {
        // 				EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I1: Floats not yet implemented!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                 return;
        // 			}
        // 			int xSource = Assembler.Stack.Pop().Size;
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
        // 			Assembler.Stack.Push(new StackContent(1, typeof(byte)));
        // 		}
        // 	}
        // }

    }
}
