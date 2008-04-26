using System;
using System.Linq;
using System.Reflection;


namespace Indy.IL2CPU.IL.X86LinqTest.CustomImplementations.System.Diagnostics {
	public static class DebugImplRefs {
		public static readonly MethodBase WriteLineRef;
		public static readonly MethodBase WriteLineIfRef;
		public static readonly Assembly RuntimeAssemblyDef;

		static DebugImplRefs() {
			Type xType = typeof(DebugImpl);
			foreach (FieldInfo xField in typeof(DebugImplRefs).GetFields()) {
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