using System;
using System.Linq;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class ObjectUtilities {
		public static bool IsDelegate(TypeDefinition aType) {
			if (aType.FullName == "System.Object") {
				return false;
			}
			if(aType.BaseType.FullName == "System.Delegate") {
				return true;
			}
			if (aType.BaseType.FullName == "System.Object") {
				return false;
			}
			return IsDelegate(Engine.GetDefinitionFromTypeReference(aType.BaseType));
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