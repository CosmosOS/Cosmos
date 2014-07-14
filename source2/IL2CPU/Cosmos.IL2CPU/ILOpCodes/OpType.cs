using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpType : ILOpCode {
    public readonly Type Value;

    public OpType(Code aOpCode, int aPos, int aNextPos, Type aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      Value = aValue;
    }

    public override int NumberOfStackPops
    {
      get
      {
        switch (OpCode)
        {
          case Code.Initobj:
            return 1;
          case Code.Ldelema:
            return 2;
          case Code.Newarr:
            return 1;
          case Code.Box:
            return 1;
          case Code.Stelem:
            return 3;
          case Code.Ldelem:
            return 2;
          case Code.Isinst:
            return 1;
          case Code.Castclass:
            return 1;
          case Code.Constrained:
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
          case Code.Initobj:
            return 0;
          case Code.Ldelema:
            return 1;
          case Code.Newarr:
            return 1;
          case Code.Box:
            return 1;
          case Code.Stelem:
            return 0;
          case Code.Ldelem:
            return 1;
          case Code.Isinst:
            return 1;
          case Code.Castclass:
            return 1;
          case Code.Constrained:
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
        case Code.Initobj:
          StackPopTypes[0] = typeof(void*);
          return;
        case Code.Ldelema:
          StackPopTypes[1] = Value.MakeArrayType();
          StackPushTypes[0] = typeof(void*);
          return;
        case Code.Box:
          StackPopTypes[0] = Value;
          if (Value.IsPrimitive)
          {
            StackPushTypes[0] = typeof (Box<>).MakeGenericType(Value);
          }
          else
          {
            StackPushTypes[0] = Value;
          }
          return;
        case Code.Newarr:
          StackPushTypes[0] = Value.MakeArrayType();
          return;
        case Code.Stelem:
          StackPopTypes[0] = Value;
          StackPopTypes[2] = Value.MakeArrayType();
          return;
        case Code.Ldelem:
          StackPopTypes[1] = Value.MakeArrayType();
          StackPushTypes[0] = Value;
          return;
        case Code.Isinst:
          if (Value.IsGenericType && Value.GetGenericTypeDefinition() == typeof(Nullable<>)) 
          {
            StackPopTypes[0] = typeof(Box<>).MakeGenericType(Value.GetGenericArguments()[0]); 
          }
          else if (Value.IsValueType)
          {
            StackPopTypes[0] = typeof(Box<>).MakeGenericType(Value);
          }
          else
          {
            StackPopTypes[0] = Value;
          }
          return;
        case Code.Castclass:
          if (Value.IsGenericType && Value.GetGenericTypeDefinition() == typeof(Nullable<>))
          {
            StackPushTypes[0] = typeof(Box<>).MakeGenericType(Value.GetGenericArguments()[0]);
          }
          else if (Value.IsValueType)
          {
            StackPushTypes[0] = typeof(Box<>).MakeGenericType(Value);
          }
          else
          {
            StackPushTypes[0] = Value;
          }
          return;
        default:
          break;
      }
    }
  }
}
