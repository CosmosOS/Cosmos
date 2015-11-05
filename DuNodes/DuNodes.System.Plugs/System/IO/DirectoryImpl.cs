using System.IO;
using Cosmos.IL2CPU.Plugs;

namespace DuNodes.System.Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        //public static DirectoryInfo CreateDirectory(string path)
        //{
        //    return new DirectoryInfo(path);
        //}
    }
}