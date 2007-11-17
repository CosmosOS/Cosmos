using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class EventHandlerImplRefs {
		public static readonly AssemblyDefinition RuntimeAssemblyDef;
		public static readonly MethodDefinition CtorRef;
		public static readonly MethodDefinition GetInvokeMethodRef;
		public static readonly MethodDefinition MulticastInvokeRef;

		static EventHandlerImplRefs() {
			RuntimeAssemblyDef = AssemblyFactory.GetAssembly(typeof(EventHandlerImpl).Assembly.Location);
			TypeDefinition xType = null;
			foreach (ModuleDefinition xMod in RuntimeAssemblyDef.Modules) {
				if (xMod.Types.Contains(typeof(EventHandlerImpl).FullName)) {
					xType = xMod.Types[typeof(EventHandlerImpl).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("EventHandlerImpl type not found!");
			}
			foreach (FieldInfo xField in typeof(EventHandlerImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodDefinition xTempMethod = xType.Methods.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length)).FirstOrDefault();
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on DelegateImpl!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}
