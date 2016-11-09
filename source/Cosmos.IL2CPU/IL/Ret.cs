using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ret)]
    public class Ret : ILOp
    {
        public Ret(Cosmos.Assembler.Assembler aAsmblr) : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            //TODO: Return
            Jump_End(aMethod);
            // Need to jump to end of method. Assembler can emit this label for now
            //XS.Jump(MethodFooterOp.EndOfMethodLabelNameNormal);
        }
    }
}
