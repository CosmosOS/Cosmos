using System;
using System.Linq;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class ObjectUtilities {
		public static uint GetObjectStorageSize(TypeDefinition aType) {
			uint xResult = ObjectImpl.FieldDataOffset;
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