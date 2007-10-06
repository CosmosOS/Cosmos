using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System {
	public static class ByteImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition ToStringRef;

		static ByteImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(ByteImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(ByteImpl).FullName)) {
					xType = xMod.Types[typeof(ByteImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("ByteImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(ByteImplRefs).GetFields()) {
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