using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Indy.IL2CPU.IL.X86 {
	public class X86OpCodeMap: OpCodeMap {
		protected override MethodHeaderOp GetMethodHeaderOp() {
			return new X86MethodHeaderOp();
		}


		protected override Assembly ImplementationAssembly {
			get {
				return typeof(X86OpCodeMap).Assembly;
			}
		}
	}
}
