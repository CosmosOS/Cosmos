using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Indy.IL2CPU.IL.X86.Win32 {
	public static class RuntimeEngineImplRefs {
		public static readonly Assembly RuntimeAssemblyDef;
		public static readonly MethodBase Heap_InitializeRef;
		public static readonly MethodBase Heap_AllocNewObjectRef;
		public static readonly MethodBase Heap_ShutdownRef;
		public static readonly MethodBase ExitProcessRef;
		public static readonly MethodBase Heap_FreeRef;

		static RuntimeEngineImplRefs() {
			Type xType = typeof(RuntimeEngineImpl);
			foreach (FieldInfo xField in typeof(RuntimeEngineImplRefs).GetFields()) {
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
