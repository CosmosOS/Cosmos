using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
  public abstract class ILOp : Cosmos.IL2CPU.ILOp {
		protected ILOp(ILOpCode aOpCode):base(aOpCode)
		{
		}

    //TODO: remove this when all descendants implement this
    public override void Execute(UInt32 aMethodUID) {
    }
  }
}
