using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpInt64 : ILOpCode {
    public readonly UInt64 Value;

    public OpInt64(Code aOpCode, int aPos, int aNextPos, UInt64 aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }
  }
}
