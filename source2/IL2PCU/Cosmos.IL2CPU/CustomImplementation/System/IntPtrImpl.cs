using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
  [Plug(Target = typeof(IntPtr))]
  public static class IntPtrImpl {
    //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
    public static string ToString(IntPtr aThis) {
      return "<IntPtr>";
    }
    //}
  }
}