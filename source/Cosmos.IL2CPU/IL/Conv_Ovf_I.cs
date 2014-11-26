using System;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_Ovf_I )]
    public class Conv_Ovf_I : ILOp
    {
        public Conv_Ovf_I( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            //TODO: What if the last ILOp in a method was Conv_Ovf_I ?
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            var xSourceIsFloat = TypeIsFloat(xSource);
            switch (xSourceSize)
            {
                case 8:
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        //public override void Execute( MethodInfo aMethod, ILOpCode aOpCode, ILOpCode aNextOpCode )
        //{
        //    var xSource = Assembler.Stack.Pop();
        //    switch( xSource.Size )
        //    {
        //        case 1:
        //        case 2:
        //        case 4:
        //            {
        //                new CPUx86.Noop();
        //                break;
        //            }
        //        case 8:
        //            {
        //                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                new CPUx86.SignExtendAX { Size = 32 };
        //                //all bits of EDX == sign (EAX)
        //                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        //                //must be equal to EDX
        //                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EDX };
        //                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = AssemblerNasm.TmpPosLabel( aMethod, aNextOpCode ) };
        //                //equals
        //                new CPUx86.Interrupt { DestinationValue = 4 };
        //                break;

        //            }
        //        default:
        //            //EmitNotImplementedException( Assembler, GetServiceProvider(), "Conv_Ovf_I: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
        //            throw new NotImplementedException(); 
        //    }
        //    Assembler.Stack.Push( new StackContent( 4, true, false, false ) );
        //}


        // using System;
        // using System.IO;
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Conv_Ovf_I)]
        // 	public class Conv_Ovf_I: Op {
        // 		private readonly string NextInstructionLabel;
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_Ovf_I(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 				NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			var xSource = Assembler.Stack.Pop();
        // 			switch (xSource.Size)
        // 			{
        // 			case 1:
        // 			case 2:
        // 			case 4:
        // 				{
        // 					new CPUx86.Noop();
        // 					break;
        // 				}
        // 			case 8:
        // 				{
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //                     new CPUx86.SignExtendAX { Size = 32 };
        // 					//all bits of EDX == sign (EAX)
        //                     new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
        // 					//must be equal to EDX
        //                     new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EDX };
        //                     new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = NextInstructionLabel };
        // 					//equals
        // 					new CPUx86.Interrupt{DestinationValue=4};
        // 					break;
        // 
        // 				}
        // 			default:
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_Ovf_I: SourceSize " + xSource + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                 break;
        // 			}
        // 			Assembler.Stack.Push(new StackContent(4, true, false, false));
        // 		}
        // 	}
        // }

    }
}
