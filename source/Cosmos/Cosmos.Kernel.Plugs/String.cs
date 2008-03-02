using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target = typeof(string))]
	public class String {
		/*public int IndexOf(char c)
		{
			// TODO: We can't get 'this'
			//string me = ToString();
			//for (int i = 0; i < me.Length; i++)
			//{
			//    if (me[i] == c)
			//        return i;
			//}
			return -1;
		}*/

		public static string Concat(string aStrA, string aStrB) {
			char[] xChars = new char[aStrA.Length + aStrB.Length];
            int xPos = 0;
            for (int i = 0; i < aStrA.Length; i++)
            {
                xChars[xPos++] = aStrA[i];
            }
            for (int i = 0; i < aStrB.Length; i++)
            {
                xChars[xPos++] = aStrB[i];
            }
			return new global::System.String(xChars);
		}

		public static string Concat(string aStrA, string aStrB, string aStrC) {
			if (aStrA == null)
				aStrA = string.Empty;
			if (aStrB == null)
				aStrB = string.Empty;
			if (aStrC == null)
				aStrC = string.Empty;
			char[] xChars = new char[aStrA.Length + aStrB.Length + aStrC.Length];
            int xPos = 0;
            for (int i = 0; i < aStrA.Length; i++)
            {
                xChars[xPos++] = aStrA[i];
            }
            for (int i = 0; i < aStrB.Length; i++)
            {
                xChars[xPos++] = aStrB[i];
            }
            for (int i = 0; i < aStrC.Length; i++)
            {
                xChars[xPos++] = aStrC[i];
            }

			return new global::System.String(xChars);
		}

		public static string Concat(string aStrA, string aStrB, string aStrC, string aStrD) {
			if (aStrA == null)
				aStrA = string.Empty;
			if (aStrB == null)
				aStrB = string.Empty;
			if (aStrC == null)
				aStrC = string.Empty;
			if (aStrD == null)
				aStrD = string.Empty;
			char[] xChars = new char[aStrA.Length + aStrB.Length + aStrC.Length + aStrD.Length];
            int xPos = 0;
			for (int i = 0; i < aStrA.Length; i++) {
				xChars[xPos++] = aStrA[i];
			}
			for (int i = 0; i < aStrB.Length; i++) {
                xChars[xPos++] = aStrB[i];
			}
			for (int i = 0; i < aStrC.Length; i++) {
                xChars[xPos++] = aStrC[i];
			}
			for (int i = 0; i < aStrD.Length; i++) {
                xChars[xPos++] = aStrD[i];
			}
			return new global::System.String(xChars);
		}

		public static bool EqualsHelper(string aStrA, string aStrB) {
			return aStrA.CompareTo(aStrB) == 0;
		}
	}
}
