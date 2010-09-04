using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(Int32))]
	public static class Int32Impl {
        public static string ToString(ref int aThis)
        {
      return Int32Impl2.GetNumberString(aThis);
		}
  }

  // See comment in UInt32Impl
  public static class Int32Impl2 {
    public static string GetNumberString(int aValue) {
			bool xIsNegative = false;
			if (aValue < 0) {
				xIsNegative = true;
				aValue *= -1;
			}
			var xResult = UInt32Impl2.GetNumberString((uint)aValue, xIsNegative);
            if(xResult==null) {
                return UInt32Impl2.GetNumberString((uint)aValue, xIsNegative);
            }
		    return xResult;
		}
	}
}