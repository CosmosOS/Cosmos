//#define COSMOSDEBUG
using System.IO;
using IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System2_Plugs.System.IO
{
    [Plug(Target = typeof(FileInfo))]
    public static class FileInfoImpl
    {
        public static bool get_Exists(FileInfo aThis)
        {
            string aPath = aThis.FullName;
            Global.mFileSystemDebugger.SendInternal($"FileInfo.Exists : aPath = {aPath}");
            return VFSManager.FileExists(aPath);
        }

        /* Optimize this: CosmosVFS should expose an attribute without the need to open the file for reading... */
        public static long get_Length(FileInfo aThis)
        {
            using (var xFs = aThis.OpenRead())
            {
                return xFs.Length;
            }
        }
    }
}
