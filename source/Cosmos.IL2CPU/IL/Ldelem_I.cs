using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldelem_I )]
    public class Ldelem_I : ILOp
    {
        public Ldelem_I( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Ldelem_Ref.Assemble(Assembler, 4, true, aMethod, aOpCode, DebugEnabled);
        }
    }
}
