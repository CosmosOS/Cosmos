using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  public class ILOpProfiler : Cosmos.IL2CPU.ILOp {
    public ILOpProfiler(ILOpCode aOpCode)
      : base(aOpCode) {
    }

    public override void Execute(UInt32 aMethodUID) { }
  }
}
