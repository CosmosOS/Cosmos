using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {

  public class Halt: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      new CPUx86.Halt();
    }

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CosCPUx86.Halt();
    }
  }
}
