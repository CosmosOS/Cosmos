using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class StringImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition get_Length_MetalRef;
		public static readonly MethodDefinition get_Chars_MetalRef;
		public static readonly MethodDefinition get_Length_NormalRef;

		static StringImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(StringImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(StringImpl).FullName)) {
					xType = xMod.Types[typeof(StringImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("StringImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(StringImplRefs).GetFields()) {
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
