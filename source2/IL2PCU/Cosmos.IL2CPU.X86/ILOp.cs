using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOp : Cosmos.IL2CPU.ILOp {
    protected new readonly Assembler Assembler;

    protected ILOp(Cosmos.IL2CPU.Assembler aAsmblr)
      : base(aAsmblr) {
      Assembler = (Assembler)aAsmblr;
      OldAsmblr = ((CPU.Assembler)Indy.IL2CPU.Assembler.Assembler.CurrentInstance.Peek());
		}

    //TODO: Temp hack for old assembler. Remove later.
    protected readonly CPU.Assembler OldAsmblr;

  }
}
