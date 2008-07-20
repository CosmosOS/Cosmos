using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target=typeof(Int64))]
    public  class Int64Impl
    {
        public static string ToString(ref long aThis)
        {
			return GetNumberString(aThis);
        }

		public static string GetNumberString(long aValue) {
			bool xIsNegative = false;
			if (aValue < 0) {
				xIsNegative = true;
				aValue *= -1;
			}
			return UInt64Impl.GetNumberString((ulong)aValue, xIsNegative);
		}
    }
}
