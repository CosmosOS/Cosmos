using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
  public class AssemblerNasm : Assembler {

    protected override void InitILOps() {
      InitILOps(typeof(ILOp));
    }

  }
}
