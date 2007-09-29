using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class ArrayImplRefs {
		static ArrayImplRefs() {
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeEngineRefs.RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(ArrayImpl).FullName)) {
					xType = xMod.Types[typeof(ArrayImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("ArrayImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(ArrayImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodDefinition xTempMethod = xType.Methods.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length)).FirstOrDefault();
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on ArrayImpl!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}

		//public static readonly MethodDefinition InitArrayWithReferenceTypesRef;
	}
}
