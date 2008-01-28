using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Screen {
	public class Text {
		public const int Columns = 80;
		public const int Lines = 24;
		public const uint VideoAddr = 0xB8000;
		private static byte Color = 7;

		public static unsafe void Clear() {
			for (int i = 0; i < Columns * (Lines + 1); i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i*2;
				*xScreenPtr = 0;
				xScreenPtr += 1;
				*xScreenPtr = Color;
			}
		}

		public static unsafe void ScrollUp() {
			for (int i = 0; i < Columns * (Lines); i++) {
				byte* xScreenPtr = (byte*)(VideoAddr + (i * 2));
				*xScreenPtr = *(xScreenPtr + (Columns * 2));
				xScreenPtr += 1;
				*xScreenPtr = *(xScreenPtr + (Columns * 2));
			}
			for (int i = 0; i < Columns; i++) {
				byte* xScreenPtr = (byte*)(VideoAddr + (i + Lines * Columns) * 2);
				*xScreenPtr = 0;
				xScreenPtr += 1;
				*xScreenPtr = Color;
			}
		}

		public unsafe static void PutChar(int aLine, int aPos, char aChar) {
			int xScreenOffset = ((aPos + (aLine * 80)) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)aChar;
			*xScreenPtr = (byte)(xVal & 0xFF);
			xScreenPtr += 1;
			*xScreenPtr = Color;
		}

		public static void SetColors(ConsoleColor foreground, ConsoleColor background) {
			Color = (byte)((byte)foreground | ((byte)background << 4));
		}
	}
}
