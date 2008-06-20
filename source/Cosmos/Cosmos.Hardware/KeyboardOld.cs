using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Hardware {
	public class KeyboardOld {
		private class KeyMapping {
			public uint Scancode;
			public char Value;
			public KeyMapping(uint aScanCode, char aValue) {
				Scancode = aScanCode;
				Value = aValue;
			}
		}
		private static Queue<uint> mBuffer;
		private const int BufferSize = 64;
		private static bool mEscaped;
		private static List<KeyMapping> mKeys;
		private static bool mShiftState;

		protected static void HandleScancode(byte aScancode, bool aReleased) {
			uint xTheScancode = aScancode;
			if (mEscaped) {
				xTheScancode = (ushort)(xTheScancode << 8);
				mEscaped = false;
			}
			switch (xTheScancode) {
                case 0x36:
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
									//DebugUtil.SendError("Keyboard", "error while getting scancode character!");
								} else {
									//DebugUtil.SendDoubleNumber("Keyboard", "Scancode and Char", xTheScancode, 32, xTheChar, 16);
								}
								mBuffer.Enqueue(xTheScancode);
							}
						}
						break;
					}
			}
			//DebugUtil.SendKeyboardEvent(xTheScancode, aReleased);
		}

		// Can merge HandleScancode after we remove old code
		// Remove the static.. Make it a real class
		protected static void ByteReceived(byte aValue) {
			bool xReleased = (aValue & 0x80) == 0x80;
			if (xReleased) {
				aValue = (byte)(aValue ^ 0x80);
			}
			HandleScancode(aValue, xReleased);
		}
		private static void CheckInit() {
			if (mBuffer == null) {
				mBuffer = new Queue<uint>(BufferSize);

				// Old
				Keyboard.Initialize(HandleScancode);
				// New
				// TODO: Need to add support for mult keyboards. ie one in PS2 and one in USB, or even more
				//var xKeyboard = (HW.SerialDevice)(HW.Device.Find(HW.Device.DeviceType.Keyboard)[0]);
				//xKeyboard.ByteReceived += new HW.SerialDevice.ByteReceivedDelegate(ByteReceived);
				// End

				mKeys = new List<KeyMapping>(128);

                //TODO: Direction Arrows, alt, ctrl, num pad, function keys, insert, home, pg up, delete, end, page down, esc, fn (for laptops)
                
                //reference: http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html#ss1.1
				#region Letters
				AddKey(0x10, 'q');
				AddKey(0x100000, 'Q');
				AddKey(0x11, 'w');
				AddKey(0x110000, 'W');
				AddKey(0x12, 'e');
				AddKey(0x120000, 'E');
				AddKey(0x13, 'r');
				AddKey(0x130000, 'R');
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

				#region digits
                //AddKey(0x1, '`');
                //AddKey(0x10000, '~');
                AddKey(0x29, '`');
                AddKey(0x290000, '~');
				AddKey(0x2, '1');
				AddKey(0x20000, '!');
				AddKey(0x3, '2');
				AddKey(0x30000, '@');
				AddKey(0x4, '3');
				AddKey(0x40000, '#');
				AddKey(0x5, '4');
				AddKey(0x50000, '$');
				AddKey(0x6, '5');
				AddKey(0x60000, '%');
				AddKey(0x7, '6');
				AddKey(0x70000, '^');
				AddKey(0x8, '7');
				AddKey(0x80000, '&');
				AddKey(0x9, '8');
				AddKey(0x90000, '*');
				AddKey(0xA, '9');
				AddKey(0xA0000, '(');
				AddKey(0xB, '0');
				AddKey(0xB0000, ')');

				#endregion

				#region Special
                AddKey(0x0E, '\u0968');     //Backspace
                AddKey(0x0E0000, '\u0968');
                AddKey(0x0F, '\t');         //Tabulator
				AddKey(0x1C, '\n');         //Enter
				AddKey(0x1C0000, '\n');
				AddKey(0x39, ' ');          //Space
				AddKey(0x390000, ' ');
				#endregion

				#region Punctuation and Signs
                AddKey(0x27, ';');
                AddKey(0x270000, ':');
                AddKey(0x28, '\'');
                AddKey(0x280000, '"');
                AddKey(0x2B, '\\');
                AddKey(0x2B0000, '|');
                AddKey(0x33, ',');
                AddKey(0x330000, '<');
				AddKey(0x34, '.');
                AddKey(0x340000, '>');
                AddKey(0x35, '/');
                AddKey(0x350000, '?');
                //AddKey(0x4A, '-');
                AddKey(0x0C, '-');
                AddKey(0x0C0000, '_');
                AddKey(0x0D, '=');
                AddKey(0x0D0000, '+');
                //AddKey(0x4E, '+');
                AddKey(0x1A, '[');
                AddKey(0x1A0000, '{');
                AddKey(0x1B, ']');
                AddKey(0x1B0000, '}');
				#endregion
			}
		}
        
		public static void Initialize() {
			CheckInit();
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
		    Console.WriteLine("Char not found!");
			aValue = '\0';
			return false;
		}

		public static char ReadChar() {
			CheckInit();
			char xResult = '\0';
			while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult))
				;
			return xResult;
		}
	}
}
