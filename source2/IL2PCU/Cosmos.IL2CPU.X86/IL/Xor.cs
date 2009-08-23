using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

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
            if( Assembler.StackContents.Peek().IsFloat )
            {
                throw new NotImplementedException( "Floats not supported" );
            }
            int xSize = Math.Max( Assembler.StackContents.Pop().Size, Assembler.StackContents.Pop().Size );
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.StackContents.Push( new StackContent( xSize ) );
        }

    }
}
