using System;
using System.Collections.Generic;
using System.Text;

namespace IL2CPU.Tests.Tests {
	public class TestEmptyMethodApp {
		public static int Main() {
			int TempInt = 5;
			return TempInt == 5 ? 0 : 1;
		}
	}
}
