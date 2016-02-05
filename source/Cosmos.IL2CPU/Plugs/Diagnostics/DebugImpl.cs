using System;

namespace Cosmos.IL2CPU.Plugs.Diagnostics {
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