using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Not )]
    public class Not : ILOp
    {
        public Not( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Not { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        }

    }
}
