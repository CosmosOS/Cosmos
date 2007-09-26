using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static class VTablesImpl {
		public struct VTable {
			public int TypeIdentifier;
			public int BaseTypeIdentifier;
			public int[] MethodIndexes;
			public int[] MethodAddresses;
		}

		private static VTable[] mTypes;
		public static bool IsInstance(int aObjectType, int aDesiredObjectType) {
			int xCurrentType = aObjectType;
			if (aObjectType == 0) {
				return true;
			}
			do {
				if (xCurrentType == aDesiredObjectType) {
					return true;
				}
				xCurrentType = mTypes[xCurrentType].BaseTypeIdentifier;
			} while (xCurrentType != 0);
			return false;
		}
	}
}
