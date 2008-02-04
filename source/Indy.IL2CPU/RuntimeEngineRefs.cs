using System;
using System.Linq;
using System.Reflection;

namespace Indy.IL2CPU {
	public static class RuntimeEngineRefs {
		public static readonly Assembly RuntimeAssemblyDef;
		public static readonly MethodBase FinalizeApplicationRef;
		public static readonly MethodBase InitializeApplicationRef;
		public static readonly MethodBase Heap_AllocNewObjectRef;

		static RuntimeEngineRefs() {
			Type xType = typeof(RuntimeEngine);
			foreach (FieldInfo xField in typeof(RuntimeEngineRefs).GetFields()) {
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