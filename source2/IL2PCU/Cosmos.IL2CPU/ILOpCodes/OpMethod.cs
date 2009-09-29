using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU.ILOpCodes {
  public class OpMethod: ILOpCode {
    public MethodBase Value;
    public uint ValueUID;
    public MethodBase BaseMethod;
    public uint BaseMethodUID;

    public OpMethod(Code aOpCode, int aPos, int aNextPos, MethodBase aValue)
      : base(aOpCode, aPos, aNextPos) {
      Value = aValue;
    }
  }
}
