using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class Keyboard {
		private static Queue<ushort> mBuffer;
		private const int BufferSize = 64;
		private static bool mEscaped;

		private void HandleScancode(byte aScancode, bool aReleased) {
			ushort xTheScancode = aScancode;
			if (mEscaped) {
				xTheScancode = (ushort)(xTheScancode + 256);
				mEscaped = false;
			}
			switch (xTheScancode) {
				case 0xE0: {
						mEscaped = true;
						break;
					}
				default: {
						if (mBuffer.Count < BufferSize) {
							mBuffer.Enqueue(xTheScancode);
						}
						break;
					}
			}
			if (!aReleased) {
				DebugUtil.SendKeyboardEvent(aScancode, aReleased);
			}
		}

		public static void Initialize() {
			mBuffer = new Queue<ushort>(BufferSize);
		}
	}
}
