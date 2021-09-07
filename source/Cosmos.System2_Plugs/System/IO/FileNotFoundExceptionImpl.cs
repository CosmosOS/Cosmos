using System.IO;

using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
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
