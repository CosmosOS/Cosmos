using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.IO;

/// <summary>
/// The BCL derives the user's home directory from the passwd database when
/// <c>HOME</c> is unset, which is where <c>System.Console</c> looks for a
/// terminfo database. The kernel has neither users nor a passwd file, so the
/// lookup reports nothing found instead of P/Invoking libSystem.Native.
/// Several assemblies compile their own copy of this type; a name-addressed
/// plug is applied to each.
/// </summary>
[Plug("System.IO.PersistedFiles")]
public static class PersistedFilesPlug
{
    [PlugMember]
    public static unsafe bool TryGetHomeDirectoryFromPasswd(byte* buf, int bufLen, out string? path)
    {
        path = null;
        return false;
    }
}
