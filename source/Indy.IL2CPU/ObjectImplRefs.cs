using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU {
	public static class ObjectImplRefs {
		static ObjectImplRefs() {
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeEngineRefs.RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(ObjectImpl).FullName)) {
					xType = xMod.Types[typeof(ObjectImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("RuntimeEngine type not found!");
			}
			Object_Ctor = xType.Methods.GetMethod("Ctor", new Type[] { typeof(IntPtr) });
			if (Object_Ctor == null)
				throw new Exception("Implementation of Object_Ctor not found!");
		}

		public static readonly MethodDefinition Object_Ctor;
	}
}