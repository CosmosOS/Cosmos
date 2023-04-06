//#define COSMOSDEBUG

using System;
using System.IO;

using IL2CPU.API.Attribs;

using Cosmos.System;

namespace Cosmos.System_Plugs.System.IO
{
   // [Plug(TargetName = "System.IO.PathInternal, System.IO.FileSystem")]
    public static class PathInternalImpl
    {
        /*
         * TODO make this better for example we can call VFSManager.GetIsCaseSensitive() and make any Filesystem
         * return this property set as true if the FileSystem is case sensitive (for example Ext3) or
         * case insensitive (for example FAT)
         */
        public static bool GetIsCaseSensitive()
        {
            Global.FileSystemDebugger.SendInternal($"GetIsCaseSensitive() called false always returned");
            return false;
        }

        public static bool IsRoot(ReadOnlySpan<char> path) =>
            (path.Length == 1 && (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar))
            || (path.Length == 2 && path[1] == Path.VolumeSeparatorChar)
            || (path.Length == 3 && path[1] == Path.VolumeSeparatorChar
                && (path[2] == Path.DirectorySeparatorChar || path[2] == Path.AltDirectorySeparatorChar));
    }
}
