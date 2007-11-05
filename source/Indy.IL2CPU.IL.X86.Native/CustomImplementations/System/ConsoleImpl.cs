using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86.CustomImplementations.System;

namespace Indy.IL2CPU.IL.X86.Native.CustomImplementations.System {
	public static class ConsoleImpl {
		public const int Columns = 80;
		public const int Lines = 24;
		public const int Attribute = 7;
		public const int VideoAddr = 0xB8000;

		private static int mCurrentLine = 0;
		private static int mCurrentChar = 0;

		private static unsafe void MoveLinesUp() {
			for (int i = 0; i < Columns * (Lines - 1); i++) {
				byte* xScreenPtr = (byte*)(VideoAddr + (i* 2));
				*xScreenPtr = *(xScreenPtr + (Columns * 2));
				xScreenPtr += 1;
				*xScreenPtr = *(xScreenPtr + (Columns * 2));
			}
			for(int i = 0; i < Columns;i++) {
				byte* xScreenPtr = (byte*)(VideoAddr + (i + Lines * Columns) * 2);
				*xScreenPtr = 0;
				xScreenPtr += 1;
				*xScreenPtr = 0;
			}
		}

		public static unsafe void Clear() {
			for (int i = 0; i < Columns * Lines * 2; i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i;
				*xScreenPtr = 0;
			}
			mCurrentLine = 0;
			mCurrentChar = 0;
		}

		private unsafe static void PutChar(int aLine, int aPos, char aChar) {
			int xScreenOffset = ((aPos + (aLine * 80)) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)aChar;
			*xScreenPtr = xVal;
			xScreenPtr += 1;
			*xScreenPtr = 7;
		}

		private static void OutputChar(char aChar) {
			PutChar(mCurrentLine, mCurrentChar, aChar);
			mCurrentChar += 1;
			if (mCurrentChar == Columns) {
				mCurrentChar = 0;
				mCurrentLine += 1;
				if (mCurrentLine == Lines) {
					Clear();
				}
			}
		}

		private static void OutputString(string aText) {
			for (int i = 0; i < aText.Length; i++) {
				Write_char_(aText[i]);
			}
		}

		public static void Write_char_(char aChar) {
			OutputChar(aChar);
		}

		public static void Write_string_(string aText) {
			OutputString(aText);
		}

		public static void WriteLine(string aLine) {
			OutputString(aLine);
			mCurrentLine += 1;
			mCurrentChar = 0;
			if (mCurrentLine == Lines) {
				MoveLinesUp();
				mCurrentLine -= 1;
				mCurrentChar = 0;
			}
		}
	}
}