using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Globalization.CultureInfo))]
	public static class CultureInfo {
		[PlugMethod(Signature="System_Void___System_Globalization_CultureInfo__cctor____")]
		public static void CCtor() {
		}
	}
}
