using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;


namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpMethod: ILOpCode {
    public MethodBase Value;
    public uint ValueUID;
    public MethodBase BaseMethod;
    public uint BaseMethodUID;

    public OpMethod(Code aOpCode, int aPos, int aNextPos, MethodBase aValue, _ExceptionRegionInfo aCurrentExceptionRegion)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionRegion) {
      Value = aValue;
    }

    public override int GetNumberOfStackPops(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Call:
        case Code.Callvirt:
          if (Value.IsStatic)
          {
            return Value.GetParameters().Length;
          }
          else
          {
            return Value.GetParameters().Length + 1;
          }
        case Code.Newobj:
          return Value.GetParameters().Length;
        case Code.Ldftn:
          return 0;
        case Code.Ldvirtftn:
          return 1;
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    public override int GetNumberOfStackPushes(MethodBase aMethod)
    {
      switch (OpCode)
      {
        case Code.Call:
        case Code.Callvirt:
          var methodInfo = Value as MethodInfo;
          if (methodInfo != null && methodInfo.ReturnType != typeof (void))
          {
            return 1;
          }
          return 0;
        case Code.Newobj:
          return 1;
        case Code.Ldftn:
          return 1;
        case Code.Ldvirtftn:
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
        case Code.Call:
        case Code.Callvirt:
          var xMethodInfo = Value as MethodInfo;
          if (xMethodInfo != null && xMethodInfo.ReturnType != typeof(void))
          {
            StackPushTypes[0] = xMethodInfo.ReturnType;
            if (StackPushTypes[0].GetTypeInfo().IsEnum)
            {
              StackPushTypes[0] = StackPushTypes[0].GetTypeInfo().GetEnumUnderlyingType();
            }
          }
          break;
        //  var xExtraOffset = 0;
        //  if (!Value.IsStatic)
        //  {
        //    StackPopTypes[0] = Value.DeclaringType;
        //    xExtraOffset++;
        //  }
        //  var xParams = Value.GetParameters();
        //  for (int i = 0; i < xParams.Length; i++)
        //  {
        //    StackPopTypes[i + xExtraOffset] = xParams[i].ParameterType;
        //  }
        //  break;
        case Code.Newobj:
          StackPushTypes[0] = Value.DeclaringType;
        //  xParams = Value.GetParameters();
        //  for (int i = 0; i < xParams.Length; i++)
        //  {
        //    StackPopTypes[i] = xParams[i].ParameterType;
        //  }
          break;
        case Code.Ldftn:
          StackPushTypes[0] = typeof (IntPtr);
          return;

        default:
          break;
      }
    }
  }
}
