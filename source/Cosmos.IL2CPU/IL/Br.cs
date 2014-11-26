using System;
using CPU = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Br )]
    public class Br : ILOp
    {
        public Br( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPU.Jump { DestinationLabel = AppAssembler.TmpBranchLabel(aMethod, aOpCode) };
        }

    }
}
