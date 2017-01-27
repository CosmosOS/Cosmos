using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldftn)]
    public class Ldftn : ILOp
    {
        public Ldftn(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpMethod = (OpMethod)aOpCode;
            XS.Push(LabelName.Get(xOpMethod.Value));
        }
    }
}
