
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System;

[Plug(typeof(Environment))]
public static class EnvironmentPlug
{
    [PlugMember]
    public static string? GetEnvironmentVariableCore(string variable)
    {
        // TODO: environment variables. The kernel has no environment block yet, so every variable
        // reads as unset: files.lua of the Lua 5.5 test suite stops on os.getenv("PATH").
        return null;
    }
}
