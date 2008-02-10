using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Screen {
	public class Text {
		public const int Columns = 80;
		public const int Lines = 24;
		public const int VideoAddr = 0xB8000;
		private const byte DefaultColor = 7;
		private static bool mInitialized = false;
		private static byte Color;

		private static void CheckInit() {
			if (!mInitialized) {
				Color = DefaultColor;
				mInitialized = true;
			}
		}

		public static unsafe void Clear() {
			CheckInit();
			for (int i = 0; i < Columns * (Lines + 1); i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i * 2;
				*xScreenPtr = 0;
				xScreenPtr += 1;
				*xScreenPtr = Color;
			}
		}

		public static unsafe void ScrollUp() {
			CheckInit();
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
			CheckInit();
			int xScreenOffset = ((aPos + (aLine * 80)) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)aChar;
			*xScreenPtr = (byte)(xVal & 0xFF);
			xScreenPtr += 1;
			*xScreenPtr = Color;
		}

		public static void SetColors(ConsoleColor foreground, ConsoleColor background) {
			CheckInit();
			Color = (byte)((byte)foreground | ((byte)background << 4));
		}
	}
}
