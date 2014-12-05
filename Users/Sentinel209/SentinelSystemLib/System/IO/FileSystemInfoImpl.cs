using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.IO;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target = typeof(global::System.IO.FileSystemInfo))]
    public static class FileSystemInfo
    {
        public static string get_FullName(global::System.IO.FileSystemInfo aThis)
        {
            return "FullName not implemented yet in FileSystemInfo plug";
        }
    }
}
