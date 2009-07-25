using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpString : ILOpCode {
    public readonly string Value;

    public OpString(Code aOpCode, string aValue)
      : base(aOpCode) {
      Value = aValue;
    }
  }
}
