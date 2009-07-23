using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class OpCodeAttribute : Attribute {
      public readonly ILOp.Code OpCode;

      public OpCodeAttribute(ILOp.Code aOpCode) {
        OpCode = aOpCode;
      }
    }
}