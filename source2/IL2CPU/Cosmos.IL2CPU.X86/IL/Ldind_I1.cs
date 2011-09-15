using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I1 )]
    public class Ldind_I1 : ILOp
    {
        public Ldind_I1( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.MoveSignExtend { DestinationReg = CPUx86.Registers.EAX, Size = 8, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.Stack.Pop();
            Assembler.Stack.Push(ILOp.Align(1, 4), typeof( int ) );
        }
    }
}