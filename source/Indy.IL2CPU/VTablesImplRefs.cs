using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU {
	public static class VTablesImplRefs {
		public static readonly Assembly RuntimeAssemblyDef;
		public static readonly Type VTablesImplDef;
		public static readonly MethodBase LoadTypeTableRef;
		public static readonly MethodBase SetTypeInfoRef;
		public static readonly MethodBase SetMethodInfoRef;
		public static readonly MethodBase GetMethodAddressForTypeRef;
		public static readonly MethodBase IsInstanceRef;

		static VTablesImplRefs() {
			VTablesImplDef = typeof(VTablesImpl);
			foreach (FieldInfo xField in typeof(VTablesImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodBase xTempMethod = VTablesImplDef.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on RuntimeEngine!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}
