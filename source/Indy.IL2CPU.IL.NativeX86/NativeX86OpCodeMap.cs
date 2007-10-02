using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86OpCodeMap: X86.X86OpCodeMap {
		protected override Type GetCustomMethodImplementationOp() {
			return typeof (NativeX86CustomMethodImplementationOp);
		}

		public override Type GetOpForCustomMethodImplementation(string aName) {
			switch (aName) {
				case "System_Void___NativeConsole_PutChar___System_Int32__System_Int32__System_Byte___": {
					return typeof (NativeX86PutCharOp);
				}
				default:
					Console.WriteLine("Trying method '{0}'", aName);
					return base.GetOpForCustomMethodImplementation(aName);
			}
		}
	}
}
