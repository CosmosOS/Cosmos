using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_U1 )]
    public class Ldind_U1 : ILOp
    {
        public Ldind_U1( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Assembler.Stack.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
#if DOTNETCOMPATIBLE
			Assembler.Stack.Push(ILOp.Align(1, 4), typeof(byte));
#else
			Assembler.Stack.Push(1, typeof(byte));
#endif
        }
    }
}