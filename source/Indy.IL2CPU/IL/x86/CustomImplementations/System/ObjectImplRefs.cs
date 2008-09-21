using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class ObjectImplRefs {
		public static readonly Assembly RuntimeAssemblyDef;

		static ObjectImplRefs() {
			Type xType = typeof(ObjectImpl);
			foreach (FieldInfo xField in typeof(ObjectImplRefs).GetFields()) {
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
