using System;
using XSharp.Compiler;
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
            XS.Pop(XSRegisters.ECX);
            new CPUx86.MoveZeroExtend { DestinationReg = CPUx86.RegistersEnum.EAX, Size = 8, SourceReg = CPUx86.RegistersEnum.ECX, SourceIsIndirect = true };
            XS.Push(XSRegisters.EAX);
        }
    }
}
