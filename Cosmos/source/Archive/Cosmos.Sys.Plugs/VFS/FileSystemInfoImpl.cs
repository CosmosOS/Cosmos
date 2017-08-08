using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.IO;

namespace Cosmos.Sys.Plugs.VFS
{
    [Plug(Target = typeof(System.IO.FileSystemInfo))]
    public static class FileSystemInfo
    {
        public static string get_FullName(System.IO.FileSystemInfo aThis)
        {
            return "FullName not implemented yet in FileSystemInfo plug";
        }
    }
}
