using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_I2 )]
    public class Conv_I2 : ILOp
    {
        public Conv_I2( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSource = Assembler.Stack.Peek();
            if (xSource.IsFloat)
            {
                new CPUx86.SSE.MoveSS { SourceReg = CPUx86.Registers.ESP, DestinationReg = CPUx86.Registers.XMM0, SourceIsIndirect = true };
                new CPUx86.SSE.ConvertSS2SI { SourceReg = CPUx86.Registers.XMM0, DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, SourceReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            }
            Assembler.Stack.Pop();
            switch( xSource.Size )
            {
                case 1:
                case 2:
                    break;
                case 4:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                case 8:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                    new CPUx86.SignExtendAX { Size = 16 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    throw new NotImplementedException( "SourceSize " + xSource + " not supported!" );
            }
            Assembler.Stack.Push(2, true, false, true);
        }


        // using System;
        // using System.IO;
        // using CPU = Cosmos.Compiler.Assembler.X86;
        // using Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_I2)]
        // 	public class Conv_I2: Op {
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_I2(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			StackContent xSource = Assembler.Stack.Pop();
        // 			if (xSource.IsFloat) {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_I2: Floats not yet implemented", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                 return;
        // 			}
        // 			switch (xSource.Size) {
        // 				case 1:
        // 				case 2:
        // 					new CPUx86.Noop();
        // 					break;
        // 				case 4:
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        // 					new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
        // 					break;
        // 				case 8:
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        //                     new CPUx86.SignExtendAX { Size = 16 };
        //                     new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 					break;
        // 				default:
        // 					throw new Exception("SourceSize " + xSource + " not supported!");
        // 			}
        // 			Assembler.Stack.Push(new StackContent(2, true, false, true));
        // 		}
        // 	}
        // }

    }
}
