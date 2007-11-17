using System;

namespace IL2CPU.Tests.Tests.Events.CustomDelegate {
	public delegate void OurDelegateTest(int aValue);
	public static class CustomDelegate {
		private static int ReturnCode;
		private static void Handler(int aValue) {
			ReturnCode -= aValue;
		}

		private static void Handler2(int aValue) {
			ReturnCode -= (aValue * 2);
		}

		private static OurDelegateTest mHandler;

		static int Main() {
			ReturnCode = 15;
			mHandler += Handler;
			mHandler += Handler2;
			mHandler(5);
			return ReturnCode;
		}
	}
}