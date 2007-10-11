using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class ObjectImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;

		static ObjectImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(ObjectImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(ObjectImpl).FullName)) {
					xType = xMod.Types[typeof(ObjectImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("ObjectImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(ObjectImplRefs).GetFields()) {
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
