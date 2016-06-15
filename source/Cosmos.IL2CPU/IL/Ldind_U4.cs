using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_U4 )]
    public class Ldind_U4 : ILOp
    {
        public Ldind_U4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
        }
    }
}
