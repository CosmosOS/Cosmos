using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(System.Console))]
	class Console {
        private static ConsoleColor _background = ConsoleColor.Black;
        private static ConsoleColor _foreground = ConsoleColor.White;

        public static ConsoleColor get_ForegroundColor()
        {
            return _foreground;
        }

        public static void set_ForegroundColor(ConsoleColor value)
        {
            _foreground = value;
            TextScreen.SetColors(_foreground, _background);
        }

        public static ConsoleColor get_BackgroundColor()
        {
            return _background;
        }

        public static void set_BackgroundColor(ConsoleColor value)
        {
            _background = value;
            TextScreen.SetColors(_foreground, _background);
        }

        public static int get_CursorLeft()
        {
            return TextScreen.CurrentChar;
        }

        public static void set_CursorLeft(int x)
        {
            TextScreen.CurrentChar = x;
        }

		public static int get_CursorTop(int y) {
			return TextScreen.CurrentLine;
		}

		public static void set_CursorTop(int y) {
			TextScreen.CurrentLine = y;
		}

		//TODO: Console uses TextWriter - intercept and plug it instead
		public static void Clear() {
			TextScreen.Clear();
		}

		public static void Write(char aChar) {
			TextScreen.WriteChar(aChar);
		}

		public static void Write(string aText) {
			for (int i = 0; i < aText.Length; i++) {
				if (aText[i] == '\n') {
					TextScreen.NewLine();
					continue;
				}
				if (aText[i] == '\r') {
					continue;
				}
				TextScreen.WriteChar(aText[i]);
			}
		}

		public static void WriteLine(string aLine) {
			Write(aLine);
			TextScreen.NewLine();
		}

		public static void Write(char[] buffer) {
			for (int i = 0; i < buffer.Length; i++)
				Write(buffer[i]);
		}

		public static void WriteLine(char[] buffer) {
			Write(buffer);
			WriteLine();
		}

		public static void WriteLine() {
			TextScreen.NewLine();
		}

		public static string ReadLine() {
			List<char> chars = new List<char>(32);
			char current;
			// HACK: convert this to "while ((current = Keyboard.ReadChar()) != '\n') {" 
			//   MTW: SOmehow an invalid opcode exception is occurring.
			while (true) {
				current = Keyboard.ReadChar();
				if (current == '\n') {
					break;
				}
                if (current == '\u0968') // Backspace
                {
                    if (chars.Count != 0)
                    {
                        chars.RemoveAt(chars.Count);
                        TextScreen.CurrentChar--;
                        TextScreen.WriteChar(' ');
                        TextScreen.CurrentChar--;
                    }
                    continue;
                }
				chars.Add(current);
				Write(current);
			}
			WriteLine();

			// HACK: Should use .ToArray here.
			char[] final = new char[chars.Count];
			for (int i = 0; i < final.Length; i++)
				final[i] = chars[i];

			return new string(final);
		}
	}
}
