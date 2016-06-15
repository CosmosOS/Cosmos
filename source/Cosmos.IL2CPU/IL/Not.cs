using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Not )]
    public class Not : ILOp
    {
        public Not( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Not(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
        }

    }
}
