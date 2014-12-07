using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I4 )]
    public class Ldind_I4 : ILOp
    {
        public Ldind_I4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push
            {
                DestinationReg = CPUx86.Registers.EAX,
                DestinationIsIndirect = true
            };
        }
    }
}