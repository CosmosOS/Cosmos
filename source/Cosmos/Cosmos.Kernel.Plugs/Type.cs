using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs {
	[Plug(Target=typeof(System.Type))]
	public static class Type {
		[PlugMethod(Signature="System_Void___System_Type__cctor____")]
		public static void CCtor() {
		}
	}
}
