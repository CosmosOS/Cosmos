using System;
using System.Linq;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class ObjectUtilities {
		public static uint GetObjectStorageSize(TypeDefinition aType) {
			uint xResult = ObjectImpl.FieldDataOffset;
			foreach (FieldDefinition xField in aType.Fields) {
				TypeDefinition xFieldType = Engine.GetDefinitionFromTypeReference(xField.FieldType);
				if (xFieldType.IsClass) {
					xResult += 4;
				} else {
					xResult+= Engine.GetFieldStorageSize(xFieldType);
				}
			}
			return xResult;
		}
	}
}