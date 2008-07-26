using System;
using System.Linq;
using Indy.IL2CPU.Assembler;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs.Assemblers
{
    public class IsVirtual8086Mode: AssemblerMethod
    {
        public override void Assemble(Assembler aAssembler) {
            new CPUx86.smsw("ax");
            new CPUx86.And("eax",
                           "1");
            new CPUx86.Push("eax");
        }
    }
}
