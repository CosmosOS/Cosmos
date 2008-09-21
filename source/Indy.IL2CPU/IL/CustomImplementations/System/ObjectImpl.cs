using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target=typeof(Object))]
	public static class ObjectImpl {
		public static string ToString(object aThis) {
			return "--object--";
		}

		public static bool InternalEquals(object a, object b) {
			return false;
		}
	}
}