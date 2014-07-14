using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpField : ILOpCode {
    public readonly FieldInfo Value;

    public OpField(Code aOpCode, int aPos, int aNextPos, FieldInfo aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }
    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Stsfld:
            return 1;
          case Code.Ldsfld:
            return 0;
          case Code.Stfld:
            return 2;
          case Code.Ldfld:
            return 1;
          case Code.Ldflda:
            return 1;
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
          case Code.Stsfld:
            return 0;
          case Code.Ldsfld:
            return 1;
          case Code.Stfld:
            return 0;
          case Code.Ldflda:
          case Code.Ldfld:
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
        case Code.Stsfld:
          StackPopTypes[0] = Value.FieldType;
          return;
        case Code.Ldsfld:
          StackPushTypes[0] = Value.FieldType;
          return;

        case Code.Stfld:
          StackPopTypes[0] = Value.FieldType;
          StackPopTypes[1] = Value.DeclaringType;
          return;
        case Code.Ldfld:
          StackPushTypes[0] = Value.FieldType;
          StackPopTypes[0] = Value.DeclaringType;
          return;
        case Code.Ldflda:
          StackPopTypes[0] = Value.DeclaringType;
          StackPushTypes[0] = typeof (IntPtr);
          return;
        default:
          break;
      }
    }
  }
}
