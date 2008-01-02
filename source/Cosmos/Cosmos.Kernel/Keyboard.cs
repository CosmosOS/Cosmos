using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class Keyboard {
		private class KeyMapping {
			public uint Scancode {
				get;
				private set;
			}

			public char Value {
				get;
				private set;
			}

			public KeyMapping(uint scanCode, char value) {
				Scancode = scanCode;
				Value = value;
			}
		}
		private static Queue<uint> mBuffer;
		private const int BufferSize = 64;
		private static bool mEscaped;
		private static List<KeyMapping> mKeys;
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
								char xTheChar;
								if (!GetCharValue(xTheScancode, out xTheChar)) {
									DebugUtil.SendError("Keyboard", "error while getting scancode character!");
								} else {
									DebugUtil.SendDoubleNumber("Keyboard", "Scancode and Char", xTheScancode, 32, xTheChar, 16);
								}
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
			mKeys = new List<KeyMapping>(128);

			#region Letters
			AddKey(0x10, 'q');
			AddKey(0x100000, 'Q');
			AddKey(0x11, 'w');
			AddKey(0x110000, 'W');
			AddKey(0x12, 'e');
			AddKey(0x120000, 'E');
			AddKey(0x13, 'r');
			AddKey(0x130000, 'r');
			AddKey(0x14, 't');
			AddKey(0x140000, 'T');
			AddKey(0x15, 'y');
			AddKey(0x150000, 'Y');
			AddKey(0x16, 'u');
			AddKey(0x160000, 'U');
			AddKey(0x17, 'i');
			AddKey(0x170000, 'I');
			AddKey(0x18, 'o');
			AddKey(0x180000, 'O');
			AddKey(0x19, 'p');
			AddKey(0x190000, 'P');

			AddKey(0x1E, 'a');
			AddKey(0x1E0000, 'A');
			AddKey(0x1F, 's');
			AddKey(0x1F0000, 'S');
			AddKey(0x20, 'd');
			AddKey(0x200000, 'D');
			AddKey(0x21, 'f');
			AddKey(0x210000, 'F');
			AddKey(0x22, 'g');
			AddKey(0x220000, 'G');
			AddKey(0x23, 'h');
			AddKey(0x230000, 'H');
			AddKey(0x24, 'j');
			AddKey(0x240000, 'J');
			AddKey(0x25, 'k');
			AddKey(0x250000, 'K');
			AddKey(0x26, 'l');
			AddKey(0x260000, 'L');

			AddKey(0x2C, 'z');
			AddKey(0x2C0000, 'Z');
			AddKey(0x2D, 'x');
			AddKey(0x2D0000, 'X');
			AddKey(0x2E, 'c');
			AddKey(0x2E0000, 'C');
			AddKey(0x2F, 'v');
			AddKey(0x2F0000, 'V');
			AddKey(0x30, 'b');
			AddKey(0x300000, 'B');
			AddKey(0x31, 'n');
			AddKey(0x310000, 'N');
			AddKey(0x32, 'm');
			AddKey(0x320000, 'M');
			#endregion

			#region Special
			AddKey(0x1C, '\n');
			AddKey(0x1C0000, '\n');
			AddKey(0x39, ' ');
			AddKey(0x390000, ' ');
			#endregion

			#region Punctuation
			AddKey(0x34, '.');
			AddKey(0x340000, '>');
			#endregion
		}

		private static void AddKey(uint p, char p_2) {
			mKeys.Add(new KeyMapping(p, p_2));
		}

		private static bool GetCharValue(uint aScanCode, out char aValue) {
			for (int i = 0; i < mKeys.Count; i++) {
				if (mKeys[i].Scancode == aScanCode) {
					aValue = mKeys[i].Value;
					return true;
				}
			}
			aValue = '\0';
			return false;
		}

		public static char ReadChar() {
			char xResult = '\0';
			while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult))
				;
			//DebugUtil.SendNumber("Keyboard", "ReadChar", xResult, 32);
			//System.Diagnostics.Debugger.Break();
			return xResult;
		}
	}
}
