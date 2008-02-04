using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Globalization.CultureInfo))]
	public static class CultureInfo {
		[PlugMethod(Signature="System_Void__System_Globalization_CultureInfo__cctor__")]
		public static void CCtor() {
		}
	}
}
