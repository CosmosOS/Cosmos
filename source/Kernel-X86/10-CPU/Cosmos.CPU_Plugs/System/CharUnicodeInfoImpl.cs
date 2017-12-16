using System;
using IL2CPU.API.Attribs;

namespace Cosmos.CPU_Plugs.System {
    [Plug(Target = typeof(global::System.Globalization.CharUnicodeInfo))]
    public static class CharUnicodeInfoImpl {
        public static void Cctor() {

        }

        public static bool InitTable() {
            return false;
        }
    }
}
