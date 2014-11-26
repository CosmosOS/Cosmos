using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.Diagnostics;

namespace Cosmos.IL2CPU.X86.PlugsLinqTest.CustomImplementations.System.Diagnostics {
	//[Plug(Target=typeof(Debug))]
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