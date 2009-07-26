using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpNone : ILOpCode {

    public OpNone(Code aOpCode, int aPos)
      : base(aOpCode, aPos) {
    }

  }
}
