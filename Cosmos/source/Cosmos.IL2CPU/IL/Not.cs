using System;

using XSharp.Common;
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

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            XS.Pop(XSRegisters.EAX);
            XS.Not(XSRegisters.EAX);
            XS.Push(XSRegisters.EAX);
        }

    }
}
