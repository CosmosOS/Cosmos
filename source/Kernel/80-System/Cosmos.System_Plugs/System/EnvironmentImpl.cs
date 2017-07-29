using System;
using Cosmos.IL2CPU.API;

namespace Cosmos.System_Plugs.System {
    [Plug(Target = typeof(Environment))]
    public static class EnvironmentImpl {
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
