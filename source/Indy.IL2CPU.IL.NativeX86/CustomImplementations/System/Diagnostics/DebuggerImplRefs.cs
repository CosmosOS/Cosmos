using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System.Diagnostics {
	public static class DebuggerImplRefs {
		public static readonly MethodDefinition BreakRef;
		public static readonly AssemblyDefinition RuntimeAssemblyDef;

		static DebuggerImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof (DebuggerImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof (DebuggerImpl).FullName)) {
					xType = xMod.Types[typeof (DebuggerImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("DebuggerImpl type not found!");
			}
			foreach (FieldInfo xField in typeof (DebuggerImplRefs).GetFields()) {
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