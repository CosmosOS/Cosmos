using System;
using System.Reflection;
using System.Reflection.Metadata;


namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpSig : ILOpCode {
    public readonly int Value;

    public OpSig(Code aOpCode, int aPos, int aNextPos, int aValue, _ExceptionRegionInfo aCurrentExceptionRegion)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionRegion) {
      Value = aValue;
    }

    public override int GetNumberOfStackPops(MethodBase aMethod)
    {
      switch (OpCode)
      {
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }

    public override int GetNumberOfStackPushes(MethodBase aMethod)
    {
      switch (OpCode)
      {
        default:
          throw new NotImplementedException("OpCode '" + OpCode + "' not implemented!");
      }
    }
  }
}
