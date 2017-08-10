using System;
using Cosmos.IL2CPU;

namespace Cosmos.IL2CPU.Profiler
{
    public class ILOp : Cosmos.IL2CPU.ILOp
    {
        public ILOp(Cosmos.Assembler.Assembler aAsmblr)
          : base(aAsmblr)
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
        {
            // Do Nothing
        }
    }
}
