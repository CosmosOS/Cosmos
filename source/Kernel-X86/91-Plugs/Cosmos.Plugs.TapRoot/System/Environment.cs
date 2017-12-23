using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Plugs.TapRoot.System {
    [Plug(Target = typeof(global::System.Environment))]
    public static class Environment {
        //        [PlugMethod(Signature = "System_Environment_OSName__System_Environment_get_OSInfo__")]
        //        public static int get_OSName() { return 0x82; }

        public static string GetResourceFromDefault(string aResource) {
            return "";
        }

        public static string GetResourceString(string aResource) {
            return "";
        }
    }
}
