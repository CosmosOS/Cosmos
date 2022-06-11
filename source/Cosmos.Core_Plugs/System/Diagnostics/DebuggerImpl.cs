using System;
using System.Diagnostics;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics;

[Plug(Target = typeof(Debugger))]
public static class DebuggerImpl
{
    public static bool get_IsAttached() => false;

    public static void Break()
    {
        // leave empty, this is handled by a special case..
    }

    public static void Log(int aInt, string aString, string aString2) => throw new NotImplementedException();
}
