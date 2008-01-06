using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(Exception))]
	public static class ExceptionImpl {
		public static string ToString(Exception aThis) {
			return aThis.Message;
		}

		[PlugMethod(Signature = "System_String___System_Exception_GetClassName____")]
		public static unsafe string GetClassName(uint* aThis) {
			int xObjectType = (int)*aThis;
			return VTablesImpl.GetTypeName(xObjectType);
		}
	}
}