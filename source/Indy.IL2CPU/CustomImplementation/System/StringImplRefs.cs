using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class StringImplRefs {
		static StringImplRefs() {
			Type xType = typeof(StringImpl);
			foreach (FieldInfo xField in typeof(StringImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodBase xTempMethod = xType.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on StringImpl!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}

		//public static readonly MethodBase GetStorageMetalRef;
		//public static readonly MethodBase GetStorageNormalRef;
		public static readonly MethodBase GetStorage_ImplRef;
	}
}