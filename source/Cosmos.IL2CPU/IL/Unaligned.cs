using System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Unaligned)]
    public class Unaligned : ILOp
    {
        public Unaligned(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            throw new NotImplementedException("TODO: Unaligned");
        }
    }
}