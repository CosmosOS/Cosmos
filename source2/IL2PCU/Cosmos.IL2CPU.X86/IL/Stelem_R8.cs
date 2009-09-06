using System;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stelem_R8 )]
    public class Stelem_R8 : ILOp
    {
        public Stelem_R8( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Stelem_Ref.Assemble( Assembler, 8, aMethod, aOpCode );
        }

    }
}
