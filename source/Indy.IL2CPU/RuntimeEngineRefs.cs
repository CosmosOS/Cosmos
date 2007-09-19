using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class RuntimeEngineRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition FinalizeApplicationRef;
		public static readonly MethodDefinition InitializeApplicationRef;

		static RuntimeEngineRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(RuntimeEngine).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(RuntimeEngine).FullName)) {
					xType = xMod.Types[typeof(RuntimeEngine).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("RuntimeEngine type not found!");
			}
			foreach (FieldInfo xField in typeof(RuntimeEngineRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodDefinition xTempMethod = xType.Methods.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length)).FirstOrDefault();
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on RuntimeEngine!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}