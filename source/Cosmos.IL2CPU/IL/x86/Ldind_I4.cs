using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.IL.x86
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I4 )]
    public class Ldind_I4 : ILOp
    {
        public Ldind_I4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.EAX);
            XS.Push(XSRegisters.EAX, isIndirect: true);
        }
    }
}
