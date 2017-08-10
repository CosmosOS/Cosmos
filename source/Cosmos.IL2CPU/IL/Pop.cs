using System;

using XSharp.Common;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Pop)]
    public class Pop : ILOp
    {
        public Pop(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            // todo: implement exception support.
            var xSize = SizeOfType(aOpCode.StackPopTypes[0]);
            XS.Add(XSRegisters.ESP, Align((uint)xSize, 4));
        }

    }
}
