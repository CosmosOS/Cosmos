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

            byte* xScreenPtr = (byte*)VideoAddr;

			for (int i = 0; i < Columns * (Lines + 1); i++) {				
				*xScreenPtr = 0;
				xScreenPtr++;
				*xScreenPtr = Color;
                xScreenPtr++;
			}
		}

		public static unsafe void ScrollUp() {
			CheckInit();
            int Columns2 = Columns * 2;
            byte* xScreenPtr = (byte*)(VideoAddr );
			for (int i = 0; i < Columns * (Lines); i++) {
				*xScreenPtr = *(xScreenPtr + Columns2);
				xScreenPtr ++;
				*xScreenPtr = *(xScreenPtr + Columns2);
                xScreenPtr++;				
			}

            xScreenPtr = (byte*)(VideoAddr + ( Lines * Columns) * 2);	
			for (int i = 0; i < Columns; i++) {
				*xScreenPtr = 0;
				xScreenPtr ++;
				*xScreenPtr = Color;
                xScreenPtr++;
			}
		}

		public unsafe static void PutChar(int aLine, int aPos, char aChar) {
			CheckInit();
			int xScreenOffset = ((aPos + (aLine * Columns)) * 2);
            byte* xScreenPtr = (byte*)(VideoAddr + xScreenOffset);
			byte xVal = (byte)aChar;
			*xScreenPtr = xVal;
			xScreenPtr ++;
			*xScreenPtr = Color;
		}

		public static void SetColors(ConsoleColor foreground, ConsoleColor background) {
			CheckInit();
            Color = (byte)((byte)(foreground ) | ((byte)(background ) << 4));
		}
	}
}
