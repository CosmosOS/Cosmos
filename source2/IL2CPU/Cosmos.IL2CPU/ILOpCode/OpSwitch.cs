using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpSwitch : ILOpCode {
    public readonly int[] BranchLocations;

    public OpSwitch(Code aOpCode, int aPos, int aNextPos, int[] aBranchLocations, System.Reflection.ExceptionHandlingClause aCurrentExceptionHandler)
      : base(aOpCode, aPos, aNextPos, aCurrentExceptionHandler) {
      BranchLocations = aBranchLocations;
    }
  }
}
