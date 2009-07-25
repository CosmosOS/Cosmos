using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpMethod : ILOpCode {
    public readonly MethodBase Value;

    public OpMethod(Code aOpCode, MethodBase aValue)
      : base(aOpCode) {
      Value = aValue;
    }
  }
}
