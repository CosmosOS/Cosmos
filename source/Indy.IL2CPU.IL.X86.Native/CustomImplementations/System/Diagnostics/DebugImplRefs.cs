using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics {
	public static class DebugImplRefs {
		public static readonly MethodDefinition WriteLineRef;
		public static readonly MethodDefinition WriteLineIfRef;
		public static readonly AssemblyDefinition RuntimeAssemblyDef;

		static DebugImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof (DebugImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof (DebugImpl).FullName)) {
					xType = xMod.Types[typeof (DebugImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("DebugImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(DebugImplRefs).GetFields()) {
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