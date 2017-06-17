using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Diagnostics
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
