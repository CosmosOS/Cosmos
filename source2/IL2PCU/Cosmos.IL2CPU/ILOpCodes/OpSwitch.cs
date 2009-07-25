using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpSwitch : ILOpCode {
    public OpSwitch(Code aOpCode)
      : base(aOpCode) {
    }
  }
}
