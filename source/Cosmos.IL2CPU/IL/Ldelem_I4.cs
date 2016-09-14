using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_I4 )]
    public class Ldelem_I4 : ILOp
    {
        public Ldelem_I4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Ldelem_Ref.Assemble(Assembler, 4, true, aMethod, aOpCode, DebugEnabled);
        }
    }
}
