using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Dup )]
    public class Dup : ILOp
    {
        public Dup( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackContent = Assembler.Stack.Peek();
            for( int i = 0; i < ( ( xStackContent.Size / 4 ) + ( xStackContent.Size % 4 == 0 ? 0 : 1 ) ); i++ )
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceDisplacement = i * 4, SourceIsIndirect = true };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
            Assembler.Stack.Push(xStackContent.Size, xStackContent.ContentType);
        }

    }
}
