using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpDouble : ILOpCode {
    public readonly Double Value;

    public OpDouble(Code aOpCode, Double aValue)
      : base(aOpCode) {
      Value = aValue;
    }
  }
}
