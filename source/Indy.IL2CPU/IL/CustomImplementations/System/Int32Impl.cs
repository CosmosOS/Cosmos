using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(Int32))]
	public static class Int32Impl {
        public static string ToString(ref int aThis)
        {
			return GetNumberString(aThis);
		}

		public static string GetNumberString(int aValue) {
			bool xIsNegative = false;
			if (aValue < 0) {
				xIsNegative = true;
				aValue *= -1;
			}
			var xResult = UInt32Impl.GetNumberString((uint)aValue, xIsNegative);
            if(xResult==null) {
                global::System.Diagnostics.Debugger.Break();
                return UInt32Impl.GetNumberString((uint)aValue, xIsNegative);
            }
		    return xResult;
		}
	}
}