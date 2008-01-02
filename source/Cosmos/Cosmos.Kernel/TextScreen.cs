using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware.Screen;

namespace Cosmos.Kernel {
    public class TextScreen {
        public static int CurrentLine = 0;
        public static int CurrentChar = 0;

        /// <summary>
        /// Sets the console colors.
        /// </summary>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        public static void SetColors(ConsoleColor foreground, ConsoleColor background)
        {
            HW.Text.SetColors(foreground, background);
        }

        public static void NewLine() {
            CurrentLine += 1;
			CurrentChar = 0;
			if (CurrentLine > (HW.Text.Lines)) {
                HW.Text.ScrollUp();
				CurrentLine -= 1;
				CurrentChar = 0;
			}
        }

        public static void Clear() {
            HW.Text.Clear();
        }

        public static void WriteChar(char aChar) {
            HW.Text.PutChar(CurrentLine, CurrentChar, aChar);
            CurrentChar += 1;
            if (CurrentChar == HW.Text.Columns) {
                CurrentChar = 0;
                CurrentLine += 1;
                if (CurrentLine > (HW.Text.Lines)) {
                	HW.Text.ScrollUp();
                    CurrentLine -= 1;
                    CurrentChar = 0;
                }
            }
        }
    }
}
