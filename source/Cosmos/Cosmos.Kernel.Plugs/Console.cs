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
				Write (buffer[i]);
		}

		public static void WriteLine(char[] buffer) {
			Write (buffer);
			WriteLine ();
		}

		public static void WriteLine() {
            TextScreen.NewLine();
        }

		public static string ReadLine() {
			List<char> chars = new List<char> ();
			char current;
			while ((current = Keyboard.ReadChar ()) != '\n') {
				chars.Add (current);
				Write (current);
			}
			WriteLine ();
			return new string(new char[] { 'q', 'w', 'a' });
		}
	}
}
