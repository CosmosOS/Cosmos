using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class MulticastDelegateImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;

		static MulticastDelegateImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(MulticastDelegateImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(MulticastDelegateImpl).FullName)) {
					xType = xMod.Types[typeof(MulticastDelegateImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("MulticastDelegateImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(MulticastDelegateImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodDefinition xTempMethod = xType.Methods.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length)).FirstOrDefault();
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on MulticastDelegateImpl!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}
