using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
    [Plug(TargetName = "System.RuntimeType+RuntimeTypeCache", IsMicrosoftdotNETOnly = true)]
	public static class RuntimeType_RuntimeTypeCache {
		public static string GetToString(object aThis) {
			return "**Reflection is not yet supported**";
		}
	}
}