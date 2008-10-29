using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System {
	[Plug(TargetName = "System.SR, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", IsMicrosoftdotNETOnly = true)]
	public static class SRImpl {
		public static string GetString(string aString) {
			return aString;
		}

		public static string GetString(string aString, params object[] aArgs) {
			return aString;
		}
	}
}
