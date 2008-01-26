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

		public static void HandleKeyboardInterrupt() {
			if (mHandleKeyboardKey != null) {
				byte xScanCode = IOReadByte(0x60);
				bool xReleased = (xScanCode & 0x80) == 0x80;
				if (xReleased) {
					xScanCode = (byte)(xScanCode ^ 0x80);
				}
				mHandleKeyboardKey(xScanCode, xReleased);
			} else {
				DebugUtil.SendError("Keyboard", "No Keyboard Handler found!");
			}
		}
	}
}
