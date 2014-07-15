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

    public override int GetNumberOfStackPops(MethodBase aMethod)
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

    public override int GetNumberOfStackPushes(MethodBase aMethod)
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

    protected override void DoInitStackAnalysis(MethodBase aMethod)
    {
      base.DoInitStackAnalysis(aMethod);

      switch (OpCode)
      {
        case Code.Stsfld:
          StackPopTypes[0] = Value.FieldType;
          if (StackPopTypes[0].IsEnum)
          {
            StackPopTypes[0] = StackPopTypes[0].GetEnumUnderlyingType();
          }
          
          return;
        case Code.Ldsfld:
          StackPushTypes[0] = Value.FieldType;
          if (StackPushTypes[0].IsEnum)
          {
            StackPushTypes[0] = StackPushTypes[0].GetEnumUnderlyingType();
          }
          return;

        case Code.Stfld:
          //StackPopTypes[0] = Value.FieldType;
          //if (StackPopTypes[0].IsEnum)
          //{
          //  StackPopTypes[0] = StackPopTypes[0].GetEnumUnderlyingType();
          //}
          //else if (Value.DeclaringType.IsValueType)
          //{
          //  StackPopTypes[0] = typeof(void*);
          //}
          //StackPopTypes[1] = Value.DeclaringType;
          return;
        case Code.Ldfld:
          StackPushTypes[0] = Value.FieldType;
          if (StackPushTypes[0].IsEnum)
          {
            StackPushTypes[0] = StackPushTypes[0].GetEnumUnderlyingType();
          }
          else if (Value.DeclaringType.IsValueType)
          {
            StackPopTypes[0] = typeof (void*);
          }
          else
          {
            StackPopTypes[0] = Value.DeclaringType;
          }
          return;
        case Code.Ldflda:
          StackPopTypes[0] = Value.DeclaringType;
          if (StackPopTypes[0].IsEnum)
          {
            StackPopTypes[0] = StackPopTypes[0].GetEnumUnderlyingType();
          }
          StackPushTypes[0] = typeof (IntPtr);
          return;
        default:
          break;
      }
    }

    protected override void DoInterpretStackTypes(ref bool aSituationChanged)
    {
      base.DoInterpretStackTypes(ref aSituationChanged);
      switch (OpCode)
      {
        case Code.Stfld:
          if (StackPopTypes[0] == null)
          {
            return;
          }
          var expectedType = Value.FieldType;
          if (expectedType.IsEnum)
          {
            expectedType = expectedType.GetEnumUnderlyingType();
          }
          else if (Value.DeclaringType.IsValueType)
          {
            expectedType = typeof(void*);
          }
          if (StackPopTypes[0] == expectedType || StackPopTypes[0] == Value.FieldType)
          {
            return;
          }
          if (expectedType == typeof (bool))
          {
            if (StackPopTypes[0] == typeof (int))
            {
              return;
            }
          }
          throw new Exception("Wrong Poptype encountered! (Type = " + StackPopTypes[0].FullName + ")");
      }
    }
  }
}
