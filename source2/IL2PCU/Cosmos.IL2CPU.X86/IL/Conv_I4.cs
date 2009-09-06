using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I4 )]
    public class Conv_I4 : ILOp
    {
        public Conv_I4( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Pop();
            if( xSource.IsFloat )
            {
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I4: Floats not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                //throw new NotImplementedException(); 
            }
            switch( xSource.Size )
            {
                case 1:
                case 2:
                case 4:
                    {
                        new CPUx86.Noop();
                        break;
                    }
                case 8:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;

                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I4: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException(); 
            }
            Assembler.Stack.Push(4,  true, false, true );
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Indy.IL2CPU.Assembler.X86;
        // using Indy.IL2CPU.Assembler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_I4)]
        // 	public class Conv_I4: Op {
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_I4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			StackContent xSource = Assembler.Stack.Pop();
        // 			if (xSource.IsFloat) {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I4: Floats not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                 return;
        // 			}
        // 			switch (xSource.Size) {
        // 				case 1:
        // 				case 2:
        // 				case 4: {
        // 						new CPUx86.Noop();
        // 						break;
        // 					}
        // 				case 8: {
        //                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                         new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 						break;
        // 
        // 					}
        // 				default:
        //                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I4: SourceSize " + xSource + " not yet supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                     return;
        // 			}
        // 			Assembler.Stack.Push(new StackContent(4, true, false, true));
        // 		}
        // 	}
        // }

    }
}
