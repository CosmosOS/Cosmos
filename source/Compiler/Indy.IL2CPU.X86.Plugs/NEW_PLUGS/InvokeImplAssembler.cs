using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.X86.Plugs.NEW_PLUGS {
  public class InvokeImplAssembler: AssemblerMethod {
    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
      throw new NotImplementedException();
    }

    public override void AssembleNew(object aAssembler) {
      throw new NotImplementedException("todo");
    }
  }
}
