using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.IL2CPU {
    public class MethodInfo {
      public readonly MethodBase MethodBase;
      public readonly UInt32 UID;

      public MethodInfo(MethodBase aMethodBase, UInt32 aUID ) {
        MethodBase = aMethodBase;
        UID = aUID;
      }

    }
}
