
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpBranch : ILOpCode {
    public readonly int Value;

    public OpBranch(Code aOpCode, int aPos, int aNextPos, int aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }

    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Leave:
          case Code.Br:
            return 0;
          case Code.Brtrue:
            return 1;
          case Code.Brfalse:
            return 1;
          case Code.Beq:
          case Code.Ble:
          case Code.Ble_Un:
          case Code.Bne_Un:
          case Code.Bge:
          case Code.Bge_Un:
          case Code.Bgt:
          case Code.Bgt_Un:
          case Code.Blt:
          case Code.Blt_Un:
            return 2;
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
          case Code.Leave:
          case Code.Br:
            return 0;
          case Code.Brtrue:
            return 0;
          case Code.Brfalse:
            return 0;
          case Code.Beq:
          case Code.Ble:
          case Code.Ble_Un:
          case Code.Bne_Un:
          case Code.Bge:
          case Code.Bge_Un:
          case Code.Bgt:
          case Code.Bgt_Un:
          case Code.Blt:
          case Code.Blt_Un:
            return 0;
          default:
            throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
        }
      }
    }

    protected override void DoInitStackAnalysis(MethodBase aMethod)
    {
      base.DoInitStackAnalysis(aMethod);

      switch (OpCode)
      {
        case Code.Brtrue:
          StackPopTypes[0] = typeof (IntPtr);
          break;
        case Code.Brfalse:
          StackPopTypes[0] = typeof(IntPtr);
          break;
        default:
          break;
      }
    }
  }
}
