using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes
{
  public class OpVar : ILOpCode
  {
    public readonly UInt16 Value;

    public OpVar(Code aOpCode, int aPos, int aNextPos, UInt16 aValue, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler)
    {
      Value = aValue;
    }

    public override int NumberOfStackPops
    {
      get
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
    }

    public override int NumberOfStackPushes
    {
      get
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
    }

    protected override void DoInitStackAnalysis()
    {
      base.DoInitStackAnalysis();

      switch (OpCode)
      {
        case Code.Ldloca:
          StackPushTypes[0] = typeof (IntPtr);
          return;
        case Code.Ldarga:
          StackPushTypes[0] = typeof (IntPtr);
          return;
        default:
          break;
      }
    }
  }
}