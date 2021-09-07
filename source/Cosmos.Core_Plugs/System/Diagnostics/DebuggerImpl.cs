using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics
{
    [Plug(Target = typeof(global::System.Diagnostics.Debugger))]
    public static class DebuggerImpl
    {
        public static void Break()
        {
            // leave empty, this is handled by a special case..
        }
    }
}
