using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
  public class AssemblerBin : Cosmos.IL2CPU.Assembler {

    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

  }
}
