using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpInt : ILOpCode {
    public readonly Int32 Value;

    public OpInt(Code aOpCode, int aPos, int aNextPos, Int32 aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }

    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Ldc_I4:
            return 0;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }

    public override int NumberOfStackPushes
    {
      get
      {
        switch (OpCode)
        {
          case Code.Ldc_I4:
            return 1;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }
  }
}
