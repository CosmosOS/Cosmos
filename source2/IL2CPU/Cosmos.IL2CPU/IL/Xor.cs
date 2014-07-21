using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Xor )]
    public class Xor : ILOp
    {
        public Xor( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Math.Max(SizeOfType(aOpCode.StackPopTypes[0]), SizeOfType(aOpCode.StackPopTypes[1]));
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        }
    }
}