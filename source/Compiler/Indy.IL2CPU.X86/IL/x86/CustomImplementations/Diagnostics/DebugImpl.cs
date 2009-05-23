using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.Diagnostics;

namespace Indy.IL2CPU.IL.X86LinqTest.CustomImplementations.System.Diagnostics {
	[Plug(Target=typeof(Debug))]
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