using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpSingle : ILOpCode {
    public readonly Single Value;

    public OpSingle(Code aOpCode, int aPos, int aNextPos, Single aValue)
      : base(aOpCode, aPos, aNextPos) {
      Value = aValue;
    }
  }
}
