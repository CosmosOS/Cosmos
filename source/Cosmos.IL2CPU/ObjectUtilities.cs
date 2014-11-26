using System;
using System.Linq;
using System.Reflection;

namespace Cosmos.IL2CPU {
	public static class ObjectUtilities {
		public static bool IsDelegate(Type aType) {
			if (aType.FullName == "System.Object") {
				return false;
			}
			if (aType.BaseType.FullName == "System.Delegate") {
				return true;
			}
			if (aType.BaseType.FullName == "System.Object") {
				return false;
			}
			return IsDelegate(aType.BaseType);
		}

		public static bool IsArray(Type aType) {
			if (aType.FullName == "System.Object") {
				return false;
			}
			if (aType.BaseType.FullName == "System.Array") {
				return true;
			}
			if (aType.BaseType.FullName == "System.Object") {
				return false;
			}
			return IsArray(aType.BaseType);
		}
	}
}