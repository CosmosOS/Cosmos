using System;
using CPU = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Br )]
    public class Br : ILOp
    {
        public Br( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPU.Jump { DestinationLabel = AssemblerNasm.TmpBranchLabel( aMethod, aOpCode ) };
        }

    }
}
