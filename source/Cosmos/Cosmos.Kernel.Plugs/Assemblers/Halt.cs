using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {

  public class Halt: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Halt();
    }
  }
}
