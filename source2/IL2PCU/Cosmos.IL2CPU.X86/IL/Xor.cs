using System;
using CPUx86 = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Xor )]
    public class Xor : ILOp
    {
        public Xor( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            int xSize = Math.Max( Assembler.Stack.Pop().Size, Assembler.Stack.Pop().Size );
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.Stack.Push(xSize);
        }

    }
}
