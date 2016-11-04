using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.IL.x86
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Pop)]
    public class Pop : ILOp
    {
        public Pop(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            // todo: implement exception support.
            var xSize = SizeOfType(aOpCode.StackPopTypes[0]);
            XS.Add(XSRegisters.ESP, Align((uint)xSize, 4));
        }

    }
}
