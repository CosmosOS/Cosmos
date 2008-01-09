using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class VTablesImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly TypeDefinition VTablesImplDef;
		public static readonly MethodDefinition LoadTypeTableRef;
		public static readonly MethodDefinition SetTypeInfoRef;
		public static readonly MethodDefinition SetMethodInfoRef;
		public static readonly MethodDefinition GetMethodAddressForTypeRef;
		public static readonly MethodDefinition IsInstanceRef;

		static VTablesImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(VTablesImpl).Assembly.Location);
			VTablesImplDef = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(VTablesImpl).FullName)) {
					VTablesImplDef = xMod.Types[typeof(VTablesImpl).FullName];
					break;
				}
			}
			if (VTablesImplDef == null) {
				throw new Exception("RuntimeEngine type not found!");
			}
			foreach (FieldInfo xField in typeof(VTablesImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodDefinition xTempMethod = VTablesImplDef.Methods.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length)).FirstOrDefault();
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on RuntimeEngine!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}
