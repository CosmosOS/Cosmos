using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(System.Console))]
	class Console {
		//TODO: Console uses TextWriter - intercept and plug it instead
		public static void Clear() {
			TextScreen.Clear();
		}

		public static void Write(char aChar) {
			TextScreen.WriteChar(aChar);
		}

		public static void Write(string aText) {
			for (int i = 0; i < aText.Length; i++) {
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
			while (true) {
				current = Keyboard.ReadChar();
				DebugUtil.SendNumber("Console", "ReadLine, new char", current, 16);
				if (current == '\n') {
					break;
				}
				//System.Diagnostics.Debugger.Break();
				DebugUtil.SendNumber("Console", "ReadLine, new char2", current, 16);
				chars.Add(current);
				Write(current);
				DebugUtil.SendNumber("Console", "ReadLine, Char count", (uint)chars.Count, 32);
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
