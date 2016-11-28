using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Dup)]
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
              XS.Push(ESP, true, (StackSize - 1) * 4);
              //new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)((StackSize - 1) * 4) };
            }
        }

    }
}
