using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU {
	public static class GCImplementationRefs {
		public static readonly MethodBase AllocNewObjectRef;
		public static readonly MethodBase IncRefCountRef;
		public static readonly MethodBase DecRefCountRef;

		static GCImplementationRefs() {
			Type xType = typeof(GCImplementation);
			if (xType == null) {
				throw new Exception("GCImplementation type not found!");
			}
			foreach (FieldInfo xField in typeof(GCImplementationRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodBase xTempMethod = xType.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on RuntimeEngine!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}