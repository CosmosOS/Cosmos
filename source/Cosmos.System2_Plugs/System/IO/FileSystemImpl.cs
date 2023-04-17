using Cosmos.System.FileSystem.VFS;
using Cosmos.System;
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

        public static void DeleteFile(string fullPath)
        {
            Global.FileSystemDebugger.SendInternal($"DeleteFile : fullPath = {fullPath}");
            VFSManager.DeleteFile(fullPath);
        }

        public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite)
        {
            Global.FileSystemDebugger.SendInternal($"CopyFile {sourceFullPath} into {destFullPath} with overwrite {overwrite}");

            // The destination path may just be a directory into which the file should be copied.
            // If it is, append the filename from the source onto the destination directory
            if (Directory.Exists(destFullPath))
            {
                destFullPath = Path.Combine(destFullPath, Path.GetFileName(sourceFullPath));
            }

            // Copy the contents of the file from the source to the destination, creating the destination in the process
            using (var src = new FileStream(sourceFullPath, FileMode.Open))
            using (var dst = new FileStream(destFullPath, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                int xSize = (int)src.Length;
                Global.FileSystemDebugger.SendInternal($"size of {sourceFullPath} is {xSize} bytes");
                byte[] content = new byte[xSize];
                Global.FileSystemDebugger.SendInternal($"content byte buffer allocated");
                src.Read(content, 0, xSize);
                Global.FileSystemDebugger.SendInternal($"content byte buffer read");
                dst.Write(content, 0, xSize);
                Global.FileSystemDebugger.SendInternal($"content byte buffer written");
            }
        }

        public static bool FileExists(string fullPath)
        {
            if (fullPath == null)
            {
                return false;
            }

            Global.FileSystemDebugger.SendInternal($"-- CosmosFileSystem.FileExists -- : fullPath = {fullPath}");
            return VFSManager.FileExists(fullPath);
        }
    }
}