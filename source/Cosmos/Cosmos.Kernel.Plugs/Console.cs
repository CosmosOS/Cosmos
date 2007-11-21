using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(System.Console))]
	class Console {
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

		public static void WriteLine() {
            TextScreen.NewLine();
        }
	}
}
