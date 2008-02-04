using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.IL {
	public abstract class PInvokeMethodBodyOp: Op {
		public readonly MethodBase TheMethod;
		public readonly MethodInformation MethodInfo;
		public PInvokeMethodBodyOp(MethodBase aTheMethod, MethodInformation aMethodInfo)
			: base(null, aMethodInfo) {
			TheMethod = aTheMethod;
			MethodInfo = aMethodInfo;
		}
	}
}
