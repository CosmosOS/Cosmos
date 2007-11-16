using System;

namespace IL2CPU.Tests.Tests.Events.CustomDelegate {
	public delegate void OurDelegateTest(int aValue);
	public static class CustomDelegate {
		private static int ReturnCode;
		private static void Handler(int aValue) {
			ReturnCode -= aValue;
		}

		static int Main() {
			ReturnCode = 5;
			OurDelegateTest xHandler = Handler;
			xHandler(5);
			return ReturnCode;
		}
	}
}