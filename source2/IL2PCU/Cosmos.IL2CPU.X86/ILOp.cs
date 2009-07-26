using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOp : Cosmos.IL2CPU.ILOp {
    
    protected ILOp(ILOpCode aOpCode):base(aOpCode) {
      Asmblr = ((CPU.Assembler)Indy.IL2CPU.Assembler.Assembler.CurrentInstance.Peek());
		}

    protected readonly CPU.Assembler Asmblr;

  }
}
