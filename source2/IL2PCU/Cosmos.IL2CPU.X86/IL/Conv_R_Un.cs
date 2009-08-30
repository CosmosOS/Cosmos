using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Converts the unsigned integer value on top of the evaluation stack to float32 or float64.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Conv_R_Un )]
    public class Conv_R_Un : ILOp
    {
        public Conv_R_Un( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            throw new NotImplementedException(); 
            var xValue = Assembler.Stack.Pop();
            if( xValue.Size > 4 )
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Doubles not yet supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException();
            }
            else
            {
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.XMM1, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
                new CPUx86.SSE.AddSS { DestinationReg = CPUx86.Registers.XMM0, SourceReg = CPUx86.Registers.XMM1 };
                new CPUx86.SSE.MoveSS { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.XMM0 };
            }

            if( xValue.Size > 8 )
            {
                //EmitNotImplementedException( Assembler, aServiceProvider, "Size '" + xSize.Size + "' not supported (add)", aCurrentLabel, aCurrentMethodInfo, aCurrentOffset, aNextLabel );
                throw new NotImplementedException();
            }
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Conv_R_Un)]
        // 	public class Conv_R_Un: Op {
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 		public Conv_R_Un(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        // 			EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_R_Un: Not implemented at all yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        // 		}
        // 	}
        // }

    }
}
