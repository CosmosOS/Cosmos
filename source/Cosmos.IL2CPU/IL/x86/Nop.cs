using System;
using XSharp.Compiler;
using CPU = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.IL.x86
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Nop )]
    public class Nop : ILOp
    {
        public Nop( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            XS.Noop();
        }

    }
}
