using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(Target = typeof(char))]
	public static class CharImpl {
		//[PlugMethod(Signature = "System_String___System_Char_ToString____")]
		public static string ToString(ref char aThis) {
			char[] xResult = new char[1];
			xResult[0] = aThis;
			return new String(xResult);
        } // System_String__System_Char_ToString__
        
        public static char ToUpper(char aThis) {
            // todo: properly implement Char.ToUpper()
            return aThis;
        }

	    public static bool IsWhiteSpace(char aChar) {
            return aChar == ' ' || aChar == '\t';
        }
	}
}
