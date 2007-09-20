using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Indy.IL2CPU.IL {
	public abstract class PInvokeMethodBodyOp: Op {
		public readonly MethodDefinition TheMethod;
		public readonly MethodInformation MethodInfo;
		public PInvokeMethodBodyOp(MethodDefinition aTheMethod, MethodInformation aMethodInfo)
			: base(null, aMethodInfo) {
			TheMethod = aTheMethod;
			MethodInfo = aMethodInfo;
		}
	}
}
