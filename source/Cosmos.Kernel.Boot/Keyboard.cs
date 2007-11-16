using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.Boot {
	public enum KeyboardKeys: int {
		Unknown,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		J,
		K,
		L,
		M,
		N,
		O,
		P,
		Q,
		R,
		S,
		T,
		U,
		V,
		W,
		X,
		Y,
		Z,
		_0,
		_1,
		_2,
		_3,
		_4,
		_5,
		_6,
		_7,
		_8,
		_9,
		Space,
		Control,
		Alt,
		Shift,
		Enter
	}
	public static class Keyboard {
		public static bool mEscaped = false;
		
		public static KeyboardKeys[] mUnshifted;
		//public static byte[] mShifted;

		
		public static bool Shift {
			get;
			private set;
		}

		public static void Initialize() {
			mUnshifted = new KeyboardKeys[] {KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys._1, KeyboardKeys._2, KeyboardKeys._3, KeyboardKeys._4, KeyboardKeys._5, KeyboardKeys._6, KeyboardKeys._7, KeyboardKeys._8, KeyboardKeys._9, KeyboardKeys._0, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Q, KeyboardKeys.W, KeyboardKeys.Unknown, KeyboardKeys.E, KeyboardKeys.T, KeyboardKeys.Y, KeyboardKeys.U, KeyboardKeys.I, KeyboardKeys.O, KeyboardKeys.P, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Enter, KeyboardKeys.Control, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D, KeyboardKeys.F, KeyboardKeys.G, KeyboardKeys.H, KeyboardKeys.J, KeyboardKeys.K, KeyboardKeys.L, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Shift, KeyboardKeys.R, KeyboardKeys.Z, KeyboardKeys.X, KeyboardKeys.C, KeyboardKeys.V, KeyboardKeys.B, KeyboardKeys.N, KeyboardKeys.M, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Alt, KeyboardKeys.Space, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown, KeyboardKeys.Unknown};

		}

		public static void HandleInterrupt() {
			byte xScanCode = IO.ReadFromPort(0x60);
			bool xWasEscaped = false;
			DebugUtil.SendKeyboardScanCodeReceived(xScanCode);
			if(xScanCode == 0xE1){
				mEscaped = true;
			} else {
				if (mEscaped) {
					xWasEscaped = true;
					mEscaped = false;
					DebugUtil.SendMessage("Keyboard", "Escaped");
				}
			}
			if (mEscaped) {
				return;
			}
			switch (xScanCode) {
				case 0x2A: {
						Shift = true;
					DebugUtil.SendMessage("Keyboard", "Shift pressed");
						break;
					}
				case 0xAA: {
						Shift = false;
						DebugUtil.SendMessage("Keyboard", "Shift released");
						break;
					}
				default: {
						if ((xScanCode & 0x80) == 0x80) {
							if (!Shift) {
								DebugUtil.SendKeyboardCharReceived(mUnshifted[(byte)(xScanCode ^ 0x80)], true);
							} else {
								DebugUtil.SendWarning("Keyboard", "Shifted not yet supported!");
							}
						} else {
							if(Shift) {
								//DebugUtil.SendKeyboardCharReceived(mShifted[xScanCode], false);
								DebugUtil.SendWarning("Keyboard", "Shifted not yet supported!");
							} else {
								DebugUtil.SendKeyboardCharReceived(mUnshifted[xScanCode], false);
								KeyboardKeys xKey = mUnshifted[xScanCode];
								if (xKey >= KeyboardKeys.A && xKey <= KeyboardKeys.Z) {
									byte xKeyByte = (byte)(97 + (xKey - KeyboardKeys.A));
									Console.Write(new String(new char[] { (char)xKeyByte }));
								}
							}
						}
					//						} else {
//							if (Shift) {
//
//							}
//						}
						break;
					}
			}
		}
	}
}