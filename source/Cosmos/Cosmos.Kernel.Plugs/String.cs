using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(Target=typeof(string))]
    public class String
    {
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
			for (int i = 0; i < aStrA.Length; i++) {
				xChars[i] = aStrA[i];
			}
			for (int i = 0; i < aStrB.Length; i++) {
				xChars[i + aStrA.Length] = aStrB[i];
			}
			return new global::System.String(xChars);
		}

		public static bool EqualsHelper(string aStrA, string aStrB) {
			return aStrA.CompareTo(aStrB) == 0;
		}
	}
}
