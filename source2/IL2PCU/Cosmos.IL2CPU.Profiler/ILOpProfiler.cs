using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.Profiler {
  public class ILOpProfiler : Cosmos.IL2CPU.ILOp {
    protected ILOpProfiler(ILOpCode aOpCode)
      : base(aOpCode) {
    }
  }
}
