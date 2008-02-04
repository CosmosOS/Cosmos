using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Type))]
	public static class Type {
		[PlugMethod(Signature="System_Void__System_Type__cctor__")]
		public static void CCtor() {
		}
	}
}
