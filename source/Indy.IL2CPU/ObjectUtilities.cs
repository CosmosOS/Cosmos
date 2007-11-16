using System;
using System.Linq;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class ObjectUtilities {
		public static bool IsDelegate(TypeReference aType) {
			TypeDefinition xType = Engine.GetDefinitionFromTypeReference(aType);
			if (xType.FullName == "System.Object") {
				return false;
			}
			if (xType.BaseType.FullName == "System.Delegate") {
				return true;
			}
			if (xType.BaseType.FullName == "System.Object") {
				return false;
			}
			return IsDelegate(xType.BaseType);
		}

		public static int GetObjectStorageSize(TypeDefinition aType) {
			if (aType == null) {
				throw new ArgumentNullException("aType");
			}
			int xResult = ObjectImpl.FieldDataOffset;
			if (IsDelegate(aType)) {
				xResult += 8;
			}
			foreach (FieldDefinition xField in aType.Fields) {
				if (xField.IsStatic) {
					continue;
				}
				if (!xField.FieldType.IsValueType) {
					xResult += 4;
				} else {
					xResult += Engine.GetFieldStorageSize(xField.FieldType);
				}
			}
			return xResult;
		}
	}
}