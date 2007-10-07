using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU.IL.X86 {
	public abstract class X86OpCodeMap: OpCodeMap {
		protected override Type GetMethodHeaderOp() {
			return typeof(X86MethodHeaderOp);
		}

		protected override Type GetMethodFooterOp() {
			return typeof(X86MethodFooterOp);
		}

		protected override Type GetInitVmtImplementationOp() {
			return typeof(X86InitVmtImplementationOp);
		}

		protected override Assembly ImplementationAssembly {
			get {
				return typeof(X86OpCodeMap).Assembly;
			}
		}

		protected override Type GetMainEntryPointOp() {
			return typeof(X86MainEntryPointOp);
		}

		protected override Type GetPInvokeMethodBodyOp() {
			return typeof(X86PInvokeMethodBodyOp);
		}

		protected override Type GetCustomMethodImplementationProxyOp() {
			return typeof(X86CustomMethodImplementationProxyOp);
		}

		public override Mono.Cecil.MethodReference GetCustomMethodImplementation(string aOrigMethodName, bool aInMetalMode) {
			switch (aOrigMethodName) {
				case "System_Int32___System_String_get_Length____": {
						if (aInMetalMode) {
							return CustomImplementations.System.StringImplRefs.get_Length_MetalRef;
						}else {
							return CustomImplementations.System.StringImplRefs.get_Length_NormalRef;
						}
					}
				case "System_Char___System_String_get_Chars___System_Int32___": {
						if (aInMetalMode) {
							return CustomImplementations.System.StringImplRefs.get_Chars_MetalRef;
						}
						goto default;
					}
				default:
					return base.GetCustomMethodImplementation(aOrigMethodName, aInMetalMode);
			}
		}
	}
}
