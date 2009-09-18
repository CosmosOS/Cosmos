using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpBranch : ILOpCode {
    public readonly int Value;

    public OpBranch(Code aOpCode, int aPos, int aNextPos, int aValue)
      : base(aOpCode, aPos, aNextPos) {
      Value = aValue;
    }
  }
}
