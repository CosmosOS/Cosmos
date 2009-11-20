using System;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I )]
    public class Conv_I : ILOp
    {
        public Conv_I( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Peek();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX};
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }
            Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                case 2:
                case 4:
                        break;
                case 8:
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                        break;
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_I: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push(4, true, false, false);
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_I)]
        // 	public class Conv_I: Op {
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
        // 			var xSource = Assembler.Stack.Pop();
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
        // 			Assembler.Stack.Push(new StackContent(4, true, false, false));
        // 		}
        // 	}
        // }

    }
}
