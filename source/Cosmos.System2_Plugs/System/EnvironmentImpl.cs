using System;
using Cosmos.System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(Environment))]
public class EnvironmentImpl
{
    public static void Exit(int aExitCode) => Power.Shutdown();
}
