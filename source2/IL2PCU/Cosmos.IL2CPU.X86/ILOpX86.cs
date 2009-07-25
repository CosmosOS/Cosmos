using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOpX86 : Cosmos.IL2CPU.ILOp {
		protected ILOpX86(ILOpCode aOpCode):base(aOpCode)
		{
		}

    //TODO: remove this when all descendants implement this
    public override void Assemble() {
    }
  }
}
