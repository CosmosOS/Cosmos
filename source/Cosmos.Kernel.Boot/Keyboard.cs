using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.Boot {
	public static class Keyboard {
		private static string Lowercase;
		private static string Uppercase;
		private static bool mEscaped = false;
		public static bool Shift {
			get;
			private set;
		}

		public static void Initialize() {
//			Lowercase = "abcdefghijklmnopqrstuvwxyz";
//			Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		}

		public static void HandleInterrupt() {
			byte xScanCode = IO.ReadFromPort(0x60);
			//			switch(xScanCode) {
			//				case 0x2A: {
			//						Shift = true;
			//						break;
			//					}
			//				case 0xAA: {
			//					Shift = false;
			//					break;
			//				}
			//				case 0xE0: {
			//						mEscaped = true;
			//						break;
			//				}
			//				default: {
			//						if (xScanCode & 0x80) {
			//						} else {
			//							if(Shift) {
			//								
			//							}
			//						}
			//				}
			//			}
			DebugUtil.SendMessage("Keyboard", "Char received");
		}
	}
}