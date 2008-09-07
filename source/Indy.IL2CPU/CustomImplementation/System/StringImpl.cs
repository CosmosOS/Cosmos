using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
	[Plug(Target = typeof(String))]
	public static class StringImpl {

		public static string Format(IFormatProvider aFormatProvider, string aFormat, object aArg) {
			return "Format not implemented";
		}

		public static string Format(IFormatProvider aFormatProvider, string aFormat, object[] aArgs) {
			string[] xStrings = new string[1 + 2 + (aArgs.Length * 7) - 1];
			xStrings[0] = aFormat;
			xStrings[1] = "(";
			for (int i = 0; i < aArgs.Length; i++) {
				xStrings[2 + (i * 7)] = "Param";
				xStrings[3 + (i * 7)] = i.ToString();
				xStrings[4 + (i * 7)] = "=";
				xStrings[5 + (i * 7)] = "\"";
				xStrings[6 + (i * 7)] = aArgs[i].ToString();
				xStrings[7 + (i * 7)] = "\"";
				if (i < (aArgs.Length - 1)) {
					xStrings[8 + (i * 7)] = ",";
				}
			}
			xStrings[xStrings.Length - 1] = ")";
			return String.Concat(xStrings);
		}

        public static string PadHelper(string aThis, int totalWidth, char paddingChar, bool isRightPadded)
        {
            char[] cs = new char[totalWidth];

            int pos = aThis.Length;

            if (isRightPadded)
            {
                for (int i = 0; i < aThis.Length; i++)
                    cs[i] = aThis[i];

                for (int i = aThis.Length; i < totalWidth; i++)
                    cs[i] = paddingChar;
            }
            else
            {
                int offset = totalWidth - aThis.Length;
                for (int i = 0; i < aThis.Length; i++)
                    cs[i + offset] = aThis[i];

                for (int i = 0; i < offset; i++)
                    cs[i] = paddingChar;
            }

            return new string(cs);
        }

		public static string Substring(string aThis, int startpos) {
			char[] cs = new char[aThis.Length - startpos];
			int j = 0;
			for (int i = startpos; i < aThis.Length; i++)
				cs[j++] = aThis[i];

			return new string(cs);
		}

		public static string Substring(string aThis, int startpos, int length) {
			if (startpos + length > aThis.Length)
				length = aThis.Length - startpos;

			char[] cs = new char[length];

			int j = 0;
			for (int i = startpos; i < startpos + length; i++)
				cs[j++] = aThis[i];

			return new string(cs);
		}

        public static string Replace(string aThis, char oldValue, char newValue)
        {
            char[] cs = new char[aThis.Length];

            for (int i = 0; i < aThis.Length; i++)
            {
                if (aThis[i] != oldValue)
                    cs[i] = aThis[i];
                else
                    cs[i] = newValue;
            }

            return new string(cs);
        }



		// HACK: We need to redo this once char support is complete (only returns 0, -1).
		public static int CompareTo(string aThis, string other) {
			if (aThis.Length != other.Length)
				return -1;
			for (int i = 0; i < aThis.Length; i++)
				if (aThis[i] != other[i])
					return -1;
			return 0;
		}

		[PlugMethod(Enabled = false)]
		public static uint GetStorage(string aString) {
			return 0;
		}

		[PlugMethod(Enabled = false)]
		public static char[] GetStorageArray(string aString) {
			return null;
		}
	}
}