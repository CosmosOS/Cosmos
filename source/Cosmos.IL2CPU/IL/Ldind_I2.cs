using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I2 )]
    public class Ldind_I2 : ILOp
    {
        public Ldind_I2( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.ECX);
            new CPUx86.MoveSignExtend { DestinationReg = CPUx86.RegistersEnum.EAX, Size = 16, SourceReg = CPUx86.RegistersEnum.ECX, SourceIsIndirect = true };
            XS.Push(XSRegisters.EAX);
        }
    }
}
