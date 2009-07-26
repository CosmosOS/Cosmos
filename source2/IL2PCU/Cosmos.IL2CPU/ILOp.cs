using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {
  public abstract class ILOp {
		public readonly ILOpCode OpCode;
		protected ILOp(ILOpCode aOpCode) {
			OpCode = aOpCode;
		}

    // This is called execute and not assemble, as the scanner
    // could be used for other things, profiling, analysis, reporting, etc
    public abstract void Execute(UInt32 aMethodUID);
  }
}
