using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_U )]
    public class Conv_U : ILOp
    {
        public Conv_U( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackContent = Assembler.Stack.Pop();
            switch( xStackContent.Size )
            {
                case 1:
                case 2:
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
                case 4:
                    {
                        new CPUx86.Noop();
                        break;
                    }
                default:
                    //EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U: SourceSize " + xStackContent.Size + "not supported yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push(4, typeof(UIntPtr));
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_U)]
        // 	public class Conv_U: Op {
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_U(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			var xStackContent = Assembler.Stack.Pop();
        // 			switch (xStackContent.Size) {
        // 				case 1:
        // 				case 2: {
        //                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 						break;
        // 					}
        // 				case 8: {
        //                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                         new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        //                         new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 						break;
        // 					}
        // 				case 4: {
        // 						new CPUx86.Noop();
        // 						break;
        // 					}
        // 				default:
        //                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_U: SourceSize " + xStackContent.Size + "not supported yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                     return;
        // 			}
        // 			Assembler.Stack.Push(new StackContent(4, true, false, false));
        // 		}
        // 	}
        // }

    }
}
