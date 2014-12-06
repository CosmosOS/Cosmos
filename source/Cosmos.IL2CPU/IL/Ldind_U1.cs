using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_U1 )]
    public class Ldind_U1 : ILOp
    {
        public Ldind_U1( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.Registers.EAX, Size = 8, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        }
    }
}