using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Indy.IL2CPU.IL.NativeX86.CustomImplementations.System;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System {
	public static class ConsoleImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition ClearRef;
		public static readonly MethodDefinition WriteLineRef;
		public static readonly MethodDefinition WriteRef;

		static ConsoleImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(ConsoleImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(ConsoleImpl).FullName)) {
					xType = xMod.Types[typeof(ConsoleImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("ConsoleImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(ConsoleImplRefs).GetFields()) {
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