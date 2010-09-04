using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_R4 )]
    public class Conv_R4 : ILOp
    {
        public Conv_R4( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Peek();
            if (!xSource.IsFloat)
            {
                new CPUx86.Move { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSI2SS { SourceReg = CPUx86.Registers.EAX, DestinationReg = CPUx86.Registers.XMM0};
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
            }
            Assembler.Stack.Pop();

            switch (xSource.Size)
            {
                case 1:
                case 2:
                    {
                        new CPUx86.Noop();
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
                        break;
                    }
                default:
                    //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_U4: SourceSize " + xStackItem.Size + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                    throw new NotImplementedException();
            }
            Assembler.Stack.Push(4, typeof(uint));
            Assembler.Stack.Push(4, true, true, true);
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
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
        // 			//int xSource = Assembler.Stack.Pop();
        // 			//switch (xSource) {
        // 			//    case 1:
        // 			//    case 2: {
        // 			//            break;
        // 			//        }
        // 			//    case 8: {
        // 			//            new CPUx86.Pop(CPUx86.Registers_Old.EAX);
        // 			//            new CPUx86.Pop(CPUx86.Registers_Old.ECX);
        // 			//            new CPUx86.Pushd(CPUx86.Registers_Old.EAX);
        // 			//            Assembler.Stack.Push(4);
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

    }
}
