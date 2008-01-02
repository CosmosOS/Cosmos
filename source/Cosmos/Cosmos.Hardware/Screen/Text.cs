using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Screen {
	public class Text {
		public const int Columns = 80;
		public const int Lines = 24;
		public const uint VideoAddr = 0xB8000;
        private static uint Color = 7;

		public static unsafe void Clear() {
			for (int i = 0; i < Columns * Lines * 2; i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i;
				*xScreenPtr = 0;
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
				*xScreenPtr = 0;
			}
		}

		public unsafe static void PutChar(int aLine, int aPos, char aChar) {
			int xScreenOffset = ((aPos + (aLine * 80)) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)aChar;
			*xScreenPtr = (byte)(xVal & 0xFF);
			xScreenPtr += 1;
			*xScreenPtr = (byte)Color;
		}

        public static void SetColors(ConsoleColor foreground, ConsoleColor background)
        {
            Color = (uint)((byte)foreground | ((byte)background << 4));
        }
    }
}
