using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System {
    [Plug(TargetName = "System.RuntimeType+RuntimeTypeCache", IsMicrosoftdotNETOnly = true)]
	public static class RuntimeType_RuntimeTypeCache {
        //public static string GetToString(object aThis) {
        //    return "**Reflection is not yet supported**";
        //}
	}
}