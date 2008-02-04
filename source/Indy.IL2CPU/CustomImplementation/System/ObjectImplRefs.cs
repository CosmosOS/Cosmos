using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU {
	public static class ObjectImplRefs {
		static ObjectImplRefs() {
			Type xType = typeof(object);
			Object_Ctor = xType.GetMethod("Ctor", new Type[] { typeof(IntPtr) });
			if (Object_Ctor == null)
				throw new Exception("Implementation of Object_Ctor not found!");
		}

		public static readonly MethodBase Object_Ctor;
	}
}