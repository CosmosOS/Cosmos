using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpSingle : ILOpCode {
    public readonly Single Value;

    public OpSingle(Code aOpCode, Single aValue)
      : base(aOpCode) {
      Value = aValue;
    }
  }
}
