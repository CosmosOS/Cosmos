using System.IO;
using IL2CPU.API.Attribs;
using System;

namespace Cosmos.System2_Plugs.System.IO
{
    [Plug(Target = typeof(FileNotFoundException))]
    public static class FileNotFoundExceptionImpl
    {
        public static string ToString(FileNotFoundException aThis)
        {
            return "FileNotFoundException";
        }
    }
}
