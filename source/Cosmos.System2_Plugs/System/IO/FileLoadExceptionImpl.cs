using System.IO;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using System;

namespace Cosmos.System2_Plugs.System.IO
{
    [Plug(Target = typeof(FileLoadException))]
    public static class FileLoadExceptionImpl
    {
        public static string FormatFileLoadExceptionMessage(string fileName, int hResult)
        {
            throw new NotImplementedException("FormatFileLoadExceptionMessage");
        }

        public static string ToString(FileLoadException aThis)
        {
            throw new NotImplementedException("FileLoadException.ToString()");
        }
    }
}
