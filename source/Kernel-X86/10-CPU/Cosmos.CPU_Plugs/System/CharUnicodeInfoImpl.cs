using System.Globalization;

using IL2CPU.API.Attribs;

namespace Cosmos.CPU_Plugs.System {
    [Plug(typeof(CharUnicodeInfo))]
    public static class CharUnicodeInfoImpl {
        public static void Cctor() {

        }

        public static bool InitTable() {
            return false;
        }
    }
}
