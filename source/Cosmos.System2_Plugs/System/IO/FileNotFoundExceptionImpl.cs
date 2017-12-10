using System.IO;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using System;
namespace Cosmos.System2_Plugs.System.IO
{
    [Plug(Target = typeof(FileNotFoundException))]
    public static class FileNotFoundExceptionImpl
    {
        public static string ToString(FileNotFoundException aThis)
        {
            throw new NotImplementedException("FileNotFoundException.ToString()");
        }
    }
}
