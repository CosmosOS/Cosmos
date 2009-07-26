using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpVar : ILOpCode {
    public readonly UInt16 Value;

    public OpVar(Code aOpCode, int aPos, UInt16 aValue)
      : base(aOpCode, aPos) {
      Value = aValue;
    }
  }
}
