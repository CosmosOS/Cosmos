using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86.CustomImplementations.System;

namespace Indy.IL2CPU.IL.NativeX86.CustomImplementations.System {
	public static class ConsoleImpl {
		public const int Columns = 80;
		public const int Lines = 24;
		public const int Attribute = 7;
		public const int VideoAddr = 0xB8000;

		private static int mCurrentLine = 0;
		private static int mCurrentChar = 0;

		public static unsafe void Clear() {
			for (int i = 0; i < Columns * Lines * 2; i++) {
				byte* xScreenPtr = (byte*)VideoAddr;
				xScreenPtr += i;
				*xScreenPtr = 0;
			}
			mCurrentLine = 0;
			mCurrentChar = 0;
		}

		private unsafe static void PutChar(int aLine, int aPos, byte aChar) {
			int xScreenOffset = ((aPos + aLine * 80) * 2);
			byte* xScreenPtr = (byte*)((0xB8000) + xScreenOffset);
			byte xVal = (byte)((aChar + 1) & 0xFF);
			*xScreenPtr = xVal;
			xScreenPtr += 1;
			*xScreenPtr = 7;
		}

		private static void OutputLine(string aLine) {
			for (int i = 0; i < aLine.Length; i++) {
				PutChar(mCurrentLine, i, StringImpl.GetByteFromChar(aLine[i]));
			}
		}

		public static void WriteLine(string aLine) {
			if (mCurrentLine == Lines) {
				Clear();
			}
			OutputLine(aLine);
			mCurrentLine += 1;
		}
	}
}