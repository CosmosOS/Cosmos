using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpToken : ILOpCode {
    public readonly UInt32 Value;

    public OpToken(Code aOpCode, UInt32 aValue)
      : base(aOpCode) {
      Value = aValue;
    }
  }
}
