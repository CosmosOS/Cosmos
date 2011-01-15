using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I8 )]
    public class Conv_I8 : ILOp
    {
        public Conv_I8( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Peek();
            Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                case 2:
                case 4:
					if (xSource.IsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
						new CPUx86.Sub { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
					else
					{
						new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
						new CPUx86.SignExtendAX { Size = 32 };
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
						new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
					}
                    break;
                case 8:
					if (xSource.IsFloat)
					{
						new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
						new CPUx86.x87.IntStoreWithTrunc { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64 };
					}
                    break;
                default:
                    //EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push(8, true, false, true);
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_I8)]
        // 	public class Conv_I8: Op {
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_I8(ILReader aReader, MethodInformation aMethodInfo)
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
        // 				case 4:
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                     new CPUx86.SignExtendAX { Size = 32 };
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //                     break;
        // 				case 8:
        // 					new CPUx86.Noop();
        // 					break;
        // 				default:
        //                     EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I8: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                     break;
        // 			}
        // 			Assembler.Stack.Push(new StackContent(8, true, false, true));
        // 		}
        // 	}
        // }

    }
}
