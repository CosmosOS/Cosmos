using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpDouble : ILOpCode {
    public readonly Double Value;

    public OpDouble(Code aOpCode, int aPos, Double aValue)
      : base(aOpCode, aPos) {
      Value = aValue;
    }
  }
}
