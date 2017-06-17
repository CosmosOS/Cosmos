using System.IO;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(FileSystemInfo))]
    public static class FileSystemInfoImpl
    {
        public static string get_FullName(FileSystemInfo aThis)
        {
            return "FullName not implemented yet in FileSystemInfo plug";
        }
    }
}
