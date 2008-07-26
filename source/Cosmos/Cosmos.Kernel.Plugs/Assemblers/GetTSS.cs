using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class GetTSS : AssemblerMethod {
        public override void Assemble(Assembler aAssembler) {
            new CPUx86.Push("TSS_0");
        }
    }
}