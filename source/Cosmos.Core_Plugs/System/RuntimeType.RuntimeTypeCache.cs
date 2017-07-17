using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System {
    [Plug(TargetName = "System.RuntimeType+RuntimeTypeCache", IsMicrosoftdotNETOnly = true)]
	public static class RuntimeType_RuntimeTypeCache {
        //public static string GetToString(object aThis) {
        //    return "**Reflection is not yet supported**";
        //}
	}
}
