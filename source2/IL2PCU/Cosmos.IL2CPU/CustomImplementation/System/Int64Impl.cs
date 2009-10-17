using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
  // Not supported yet, so commentd out.
  //[Plug(Target = typeof(Int64))]
  public class Int64Impl {
    public static string ToString(ref long aThis) {
      return Int64Impl2.GetNumberString(aThis);
    }
  }

  // See note in UInt32Impl2
  public class Int64Impl2 {
    public static string GetNumberString(long aValue) {
      bool xIsNegative = false;
      if (aValue < 0) {
        xIsNegative = true;
        aValue *= -1;
      }
      return UInt64Impl2.GetNumberString((ulong)aValue, xIsNegative);
    }
  }
}
