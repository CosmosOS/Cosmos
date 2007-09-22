using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU.IL.X86 {
	public class X86OpCodeMap: OpCodeMap {
		protected override Type GetMethodHeaderOp() {
			return typeof(X86MethodHeaderOp);
		}

		protected override Type GetMethodFooterOp() {
			return typeof(X86MethodFooterOp);
		}


		protected override Assembly ImplementationAssembly {
			get {
				return typeof(X86OpCodeMap).Assembly;
			}
		}
		protected override Type GetCustomMethodImplementationProxyOp() {
			return typeof(X86CustomMethodImplementationProxyOp);
		}

		protected override Type GetPInvokeMethodBodyOp() {
			return typeof(X86PInvokeMethodBodyOp);
		}

		protected override Type GetRuntimeHelpers_InitializeArrayOp() {
			return typeof(X86RuntimeHelpers_InitializeArrayOp);
		}
	}
}
