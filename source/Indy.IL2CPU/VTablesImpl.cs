using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static class VTablesImpl {
		// method VTable, indexed by the type
		/// <summary>
		/// This array contains a list of types in the first dimension and a list
		/// of method indexes in the second dimension.
		/// </summary>
		private static IntPtr[][] mMethods;

	}
}
