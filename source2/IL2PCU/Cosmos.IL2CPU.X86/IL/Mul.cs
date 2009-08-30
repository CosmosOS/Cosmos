using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Mul )]
    public class Mul : ILOp
    {
        public Mul( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackContent = Assembler.Stack.Pop();
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
            if( xStackContent.IsFloat )
            {
                //EmitNotSupportedException( aAssembler, aServiceProvider, "Floats are not yet supported!", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException();
            }
            else
            {
                if( xStackContent.Size > 4 )
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Multiply { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 8 };
                    new CPUx86.Push { DestinationValue = 0 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
                else
                {
                    new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                    new CPUx86.Multiply { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32 };
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                }
            }
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Mul)]
        // 	public class Mul: Op {
        // 	    private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Mul(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		    mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			Multiply(Assembler, GetServiceProvider(),
        //                 mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        // 		}
        // 	}
        // }

    }
}
