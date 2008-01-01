using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class Keyboard {
		private struct KeyMapping {
			public uint Scancode;
			public char Value;
		}
		private static Queue<uint> mBuffer;
		private const int BufferSize = 64;
		private static bool mEscaped;
		//private static KeyMapping[] mKeys;
		private static bool mShiftState;

		private static void HandleScancode(byte aScancode, bool aReleased) {
			uint xTheScancode = aScancode;
			if (mEscaped) {
				xTheScancode = (ushort)(xTheScancode << 8);
				mEscaped = false;
			}
			switch (xTheScancode) {
				case 0x2A: {
						mShiftState = !aReleased;
						break;
					}
				default: {
						if (mShiftState) {
							xTheScancode = xTheScancode << 16;
						}
						if (mBuffer.Count < BufferSize) {
							if (!aReleased) {
								mBuffer.Enqueue(xTheScancode);
							}
						}
						break;
					}
			}
			DebugUtil.SendKeyboardEvent(xTheScancode, aReleased);
		}

		public static void Initialize() {
			mBuffer = new Queue<uint>(BufferSize);
			Hardware.Keyboard.Initialize(HandleScancode);
			//mKeys = new KeyMapping[2];
			//mKeys[0] = new KeyMapping() {
			//    Scancode = 0x1E,
			//    Value = 'a'
			//};
			//mKeys[1] = new KeyMapping() {
			//    Scancode = 0x1E0000,
			//    Value = 'A'
			//};
		}

		private static bool GetCharValue(uint aScanCode, out char aValue) {
			switch (aScanCode) {
				case 0x1E:
					aValue = 'a';
					return true;
				case 0x1E0000:
					aValue = 'A';
					return true;
				case 0x10:
					aValue = 'q';
					return true;
				case 0x11:
					aValue = 'w';
					return true;
				case 0x1c:
					aValue = '\n';
					return true;
				default:
					aValue = '\0';
					return false;
			}
			//for (int i = 0; i < mKeys.Length; i++) {
			//    if (mKeys[i].Scancode == aScanCode) {
			//        aValue = mKeys[i].Value;
			//        return true;
			//    }
			//}
		}

		public static char ReadChar() {
			char xResult = '\0';
			while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult))
				;
			return xResult;
		}
	}
}
