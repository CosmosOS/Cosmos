using System;
using System.IO;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug("System.IO.FileSystem, System.Private.CoreLib")]
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

        public static void CreateDirectory(string aDir, byte[] aSecurityDescriptor)
        {
            Directory.CreateDirectory(aDir);
        }

        public static void RemoveDirectory(string aDir, bool aRecursive)
        {
            Directory.Delete(aDir, aRecursive);
        }

        public static void DeleteFile(string aFile)
        {
            CosmosFileSystem.DeleteFile(aFile);
        }

        public static void CopyFile(string aSource, string aDesintation, bool aOverwrite)
        {
            CosmosFileSystem.CopyFile(aSource, aDesintation, aOverwrite);
        }

        public static bool FileExists(string aFile)
        {
            return CosmosFileSystem.FileExists(aFile);
        }
    }
}
