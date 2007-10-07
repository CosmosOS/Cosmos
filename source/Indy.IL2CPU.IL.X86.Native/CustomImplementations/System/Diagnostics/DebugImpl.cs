using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native.CustomImplementations.System.Diagnostics {
	public static class DebugImpl {
		public static void WriteLine(string aMessage) {
			Console.Write("DEBUG: ");
			Console.WriteLine(aMessage);
		}

		public static void WriteLineIf(bool aCondition, string aMessage) {
			if(aCondition) {
				WriteLine(aMessage);
			}
		}
	}
}