using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);
	public class Keyboard: Hardware {
		private static HandleKeyboardDelegate mHandleKeyboardKey;
		public static void Initialize(HandleKeyboardDelegate aHandleKeyboardKeyDelegate) {
			mHandleKeyboardKey = aHandleKeyboardKeyDelegate;
		}

		internal static void HandleKeyboardInterrupt() {
			if (mHandleKeyboardKey != null) {
				//mHandleKeyboardKey(
				byte xScanCode = IORead(0x60);
				mHandleKeyboardKey(xScanCode, (xScanCode & 0x80) == 0x80);
			}
		}
	}
}
