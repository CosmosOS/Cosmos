using System;

namespace IL2CPU.Tests.Tests.Events.SimpleCreateDelegate {
	public class SimpleCreateDelegate {
		public static void MyHandler(object sender, EventArgs e) {
			ReturnCode = 0;
		}

		private static int ReturnCode;
		static int Main() {
			ReturnCode = 1;
			EventHandler xDelegate = MyHandler;
			xDelegate(null, null);
			return ReturnCode;
		}
	}
}
