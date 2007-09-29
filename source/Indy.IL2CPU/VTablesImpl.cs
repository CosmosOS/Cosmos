using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU {
	public static class VTablesImpl {
		public struct VTable {
			public int BaseTypeIdentifier;
			public char[] Name;
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

		public static void LoadTypeTable(int aTypeCount) {
			mTypes = new VTable[aTypeCount];
		}

		public static void SetTypeInfo(int aType, int aBaseType, int aMethodCount, char[] aName) {
			mTypes[aType] = new VTable();
			mTypes[aType].BaseTypeIdentifier = aBaseType;
			mTypes[aType].MethodIndexes = new int[aMethodCount];
			mTypes[aType].MethodAddresses = new int[aMethodCount];
			mTypes[aType].Name = aName;
		}

		public static void SetMethodInfo(int aType, int aMethodIndex, int aMethodIdentifier, int aMethodAddress, char[] aName) {
			mTypes[aType].MethodIndexes[aMethodIndex] = aMethodIdentifier;
			mTypes[aType].MethodAddresses[aMethodIndex] = aMethodAddress;
		}

		public static int GetMethodAddressForType(int aType, int aMethodIndex) {
			VTable xTable = mTypes[aType];
			for (int i = 0; i < xTable.MethodIndexes.Length; i++) {
				if (xTable.MethodIndexes[i] == aMethodIndex) {
					return xTable.MethodAddresses[i];
				}
			}
			return 0x00000000;
		}
	}
}
