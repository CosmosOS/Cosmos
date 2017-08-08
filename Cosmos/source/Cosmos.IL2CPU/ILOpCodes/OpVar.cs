using System;
using System.Reflection;
using System.Reflection.Metadata;
using Cosmos.Debug.Symbols;

using Cosmos.IL2CPU.Extensions;

namespace Cosmos.IL2CPU.ILOpCodes
{
  public class OpVar : ILOpCode
  {
    public readonly UInt16 Value;

    public OpVar(Code aOpCode, int aPos, int aNextPos, UInt16 aValue, _ExceptionRegionInfo aCurrentExceptionRegion)
        : base(aOpCode, aPos, aNextPos, aCurrentExceptionRegion)
    {
      Value = aValue;
    }

    public override int GetNumberOfStackPops(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Ldloc:
        case Code.Ldloca:
        case Code.Ldarg:
        case Code.Ldarga:
          return 0;
        case Code.Stloc:
        case Code.Starg:
          return 1;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    public override int GetNumberOfStackPushes(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Stloc:
        case Code.Starg:
          return 0;
        case Code.Ldloc:
        case Code.Ldloca:
        case Code.Ldarg:
        case Code.Ldarga:
          return 1;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    protected override void DoInitStackAnalysis(MethodBase aMethod)
    {
      base.DoInitStackAnalysis(aMethod);

      var xArgIndexCorrection = 0;
      var xParams = aMethod.GetParameters();
      var xLocals = aMethod.GetLocalVariables();
      switch (OpCode)
      {
        case Code.Ldloc:
            StackPushTypes[0] = xLocals[Value].Type;
            if (StackPushTypes[0].GetTypeInfo().IsEnum)
            {
              StackPushTypes[0] = StackPushTypes[0].GetTypeInfo().GetEnumUnderlyingType();
            }
          return;
        case Code.Ldloca:
            StackPushTypes[0] = xLocals[Value].Type.MakeByRefType();
          return;
        case Code.Ldarg:
          if (!aMethod.IsStatic)
          {
            if (Value == 0)
            {
              StackPushTypes[0] = aMethod.DeclaringType;
              if (StackPushTypes[0].GetTypeInfo().IsEnum)
              {
                StackPushTypes[0] = StackPushTypes[0].GetTypeInfo().GetEnumUnderlyingType();
              }
              else if (StackPushTypes[0].GetTypeInfo().IsValueType)
              {
                StackPushTypes[0] = StackPushTypes[0].MakeByRefType();
              }
              return;
            }
            xArgIndexCorrection = -1;
          }
          StackPushTypes[0] = xParams[Value + xArgIndexCorrection].ParameterType;
          if (StackPushTypes[0].GetTypeInfo().IsEnum)
          {
            StackPushTypes[0] = StackPushTypes[0].GetTypeInfo().GetEnumUnderlyingType();
          }
          return;
        case Code.Ldarga:
          if (!aMethod.IsStatic)
          {
            if (Value == 0)
            {
              if (StackPushTypes[0].GetTypeInfo().IsValueType)
              {
                StackPushTypes[0] = StackPushTypes[0].MakeByRefType();
              }
              return;
            }
            xArgIndexCorrection = -1;
          }
          StackPushTypes[0] = xParams[Value + xArgIndexCorrection].ParameterType;
          StackPushTypes[0] = StackPushTypes[0].MakeByRefType();
          return;
      }
    }
  }
}
