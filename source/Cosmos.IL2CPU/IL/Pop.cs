using System;
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

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            // todo: implement exception support.
            var xSize = SizeOfType(aOpCode.StackPopTypes[0]);
            new CPUx86.Add
            {
                DestinationReg = CPUx86.Registers.ESP,
                SourceValue = ILOp.Align((uint)xSize, 4)
            };
        }

    }
}
