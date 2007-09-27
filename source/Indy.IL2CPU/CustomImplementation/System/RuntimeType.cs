using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU {
	public static class RuntimeType {
		[MethodAlias(Name="System.RuntimeType.get_Cache()")]
		public static IntPtr Cache_Get(IntPtr aThis) {
			return aThis;
		}
	}
}