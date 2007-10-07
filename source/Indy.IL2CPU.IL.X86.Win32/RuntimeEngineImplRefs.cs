using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public static class RuntimeEngineImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition Heap_InitializeRef;
		public static readonly MethodDefinition Heap_AllocNewObjectRef;
		public static readonly MethodDefinition Heap_ShutdownRef;
		public static readonly MethodDefinition ExitProcessRef;

		static RuntimeEngineImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(RuntimeEngineImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(RuntimeEngineImpl).FullName)) {
					xType = xMod.Types[typeof(RuntimeEngineImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("RuntimeEngineImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(RuntimeEngineImplRefs).GetFields()) {
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
