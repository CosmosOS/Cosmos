using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I2 )]
    public class Ldind_I2 : ILOp
    {
        public Ldind_I2( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = 0 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.DX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
            Assembler.Stack.Pop();
#if DOTNETCOMPATIBLE
            Assembler.Stack.Push(ILOp.Align(2, 4), typeof( short ) ) ;
#else
			Assembler.Stack.Push( 2, typeof( short ) );
#endif
        }
    }
}