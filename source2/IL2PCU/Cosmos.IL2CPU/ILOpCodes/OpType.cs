using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpType : ILOpCode {
    public readonly Type Value;

    public OpType(Code aOpCode, int aPos, Type aValue)
      : base(aOpCode, aPos) {
      Value = aValue;
    }
  }
}
