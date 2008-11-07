using System;
using System.Linq;
using System.Reflection;

namespace Indy.IL2CPU {
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

		public static uint GetObjectStorageSize(Type aType) {
			if (aType == null) {
				throw new ArgumentNullException("aType");
			}
			var xTypeInfo = Engine.GetTypeInfo(aType);
		    return xTypeInfo.NeedsGC
		               ? xTypeInfo.StorageSize + ObjectImpl.FieldDataOffset
		               : xTypeInfo.StorageSize;
		}
	}
}