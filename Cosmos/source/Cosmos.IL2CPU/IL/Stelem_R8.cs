using System;

using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stelem_R8 )]
    public class Stelem_R8 : ILOp
    {
        public Stelem_R8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            Stelem_Ref.Assemble(Assembler, 8, aMethod, aOpCode, DebugEnabled);
        }

    }
}
