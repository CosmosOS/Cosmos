using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpString : ILOpCode {
    public readonly string Value;

    public OpString(Code aOpCode, int aPos, int aNextPos, string aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }

    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Ldstr:
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
          case Code.Ldstr:
            return 1;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }
    
    protected override void DoInitStackAnalysis()
    {
      base.DoInitStackAnalysis();

      switch (OpCode)
      {
        case Code.Ldstr:
          StackPushTypes[0] = typeof (string);
          break;
        default:
          break;
      }
    }
  }
}
