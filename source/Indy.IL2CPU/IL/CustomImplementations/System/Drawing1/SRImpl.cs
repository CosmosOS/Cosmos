using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.CustomImplementations.System.Drawing {
	[Plug(TargetName = "System.Drawing.SR, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", IsMicrosoftdotNETOnly = true)]
	public static class SRImpl {
		public static string GetString(string aString) {
			return aString;
		}

		public static string GetString(string aString, params object[] aArgs) {
			return aString;
		}
	}
}
