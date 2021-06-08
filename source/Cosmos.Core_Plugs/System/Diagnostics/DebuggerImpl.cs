using System;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics
{
    [Plug(Target = typeof(global::System.Diagnostics.Debugger))]
    public static class DebuggerImpl
    {
        public static bool get_IsAttached()
        {
            return false;
        }
        public static void Break()
        {
            // leave empty, this is handled by a special case..
        }

        public static void Log(int aInt, string aString, string aString2)
        {
            throw new NotImplementedException();
        }
    }
}
