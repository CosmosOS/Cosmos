using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	public static class DebugHelper {
		private static bool mWritingPossible = false;
		public static void DoWriteIfPossible(string aMessage) {
			if (mWritingPossible) {
				System.Diagnostics.Debug.WriteLine(aMessage);
			}
		}

		public static void MakeWritingPossible() {
			//mWritingPossible = true;
		}
	}
}
