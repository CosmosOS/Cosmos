using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Win32OpCodeMap: X86OpCodeMap {
		protected override Type GetCustomMethodImplementationOp() {
			return typeof(Win32CustomMethodImplementationOp);
		}
	}
}
