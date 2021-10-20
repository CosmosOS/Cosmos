using System;
using System.IO;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug("System.IO.FileSystem, System.IO.FileSystem")]
    class FileSystemImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_IO_FileSystem_GetFileAttributesEx_System_String_Interop_Kernel32_GET_FILEEX_INFO_LEVELS_System_Boolean)")]
        public static int FillAttributeInfo(string aDir, ref object aObject, bool aBool)
        {
            throw new NotImplementedException();
        }

        public static bool DirectoryExists(string aDir, ref int aInt)
        {
            return Directory.Exists(aDir);
        }
    }
}
