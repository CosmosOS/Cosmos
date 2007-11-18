using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class GCImplementationRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition AllocNewObjectRef;
		public static readonly MethodDefinition IncRefCountRef;
		public static readonly MethodDefinition DecRefCountRef;

		static GCImplementationRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(GCImplementation).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(GCImplementation).FullName)) {
					xType = xMod.Types[typeof(GCImplementation).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("GCImplementation type not found!");
			}
			foreach (FieldInfo xField in typeof(GCImplementationRefs).GetFields()) {
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