using System;
using CPUx86 = Cosmos.Assembler.x86;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.IL.x86
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Xor )]
    public class Xor : ILOp
    {
        public Xor( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xSize = Math.Max(SizeOfType(aOpCode.StackPopTypes[0]), SizeOfType(aOpCode.StackPopTypes[1]));
            XS.Pop(XSRegisters.EAX);
            XS.Pop(XSRegisters.EDX);
            XS.Xor(XSRegisters.EAX, XSRegisters.EDX);
            XS.Push(XSRegisters.EAX);
        }
    }
}
