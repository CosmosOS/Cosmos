using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	public static class EventHandlerImplRefs {
		public static readonly Assembly RuntimeAssemblyDef;
		public static readonly MethodBase CtorRef;
		//public static readonly MethodBase GetInvokeMethodRef;
		//public static readonly MethodBase MulticastInvokeRef;

		static EventHandlerImplRefs() {
			Type xType = typeof(EventHandlerImpl);
			foreach (FieldInfo xField in typeof(EventHandlerImplRefs).GetFields()) {
				if (xField.Name.EndsWith("Ref")) {
					MethodBase xTempMethod = xType.GetMethod(xField.Name.Substring(0, xField.Name.Length - "Ref".Length));
					if (xTempMethod == null) {
						throw new Exception("Method '" + xField.Name.Substring(0, xField.Name.Length - "Ref".Length) + "' not found on DelegateImpl!");
					}
					xField.SetValue(null, xTempMethod);
				}
			}
		}
	}
}
