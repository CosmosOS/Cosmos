using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Dup)]
    public class Dup : ILOp
    {
        public Dup(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackContent = aOpCode.StackPopTypes[0];
            var xStackContentSize = SizeOfType(xStackContent);
            var StackSize = (int)((xStackContentSize / 4) + (xStackContentSize % 4 == 0 ? 0 : 1));

            for (int i = StackSize; i > 0; i--)
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)((StackSize - 1) * 4) };
            }
        }

    }
}