//#define COSMOSDEBUG
using System;
using System.IO;
using IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

/*
 * This is is similar to what we have done for FileSystem but they did in a weird and really unclear way this time
 * (an abstract class that inherits from an interface? What is the sense? They have not used a class for
 *  any platform but did "kludges" with partial classes).
 * In the end the more simple thing to do is plug things here some it seems only Properties will hit this code
 * while methods will call FileSystem directly... what a mess!
 */
namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(FileSystemInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(DirectoryEntry))]
    public static class FileSystemInfoImpl
    {
        [PlugMethod(Signature = "System_Boolean__System_IO_FileSystemInfo_System_IO_IFileSystemObject_get_Exists__")]
        public static bool get_Exists(FileSystemInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            Global.mFileSystemDebugger.SendInternal($"FileSystemInfo.get_Exists : fullPath = {aThis.FullName}");
            return aStorage != null;
        }

        public static FileAttributes get_Attributes(FileSystemInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"FileSystemInfo.get_Attributes : fullPath = {aThis.FullName}");
            return VFSManager.GetFileAttributes(aThis.FullName);
        }

        public static void set_Attributes(FileSystemInfo aThis, FileAttributes value)
        {
            Global.mFileSystemDebugger.SendInternal($"FileSystemInfo.set_Attributes : fullPath = {aThis.FullName} value {(int)value}");
            VFSManager.SetFileAttributes(aThis.FullName, value);
        }

        public static FileSystemInfo get_FileSystemObject(FileSystemInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            Global.mDebugger.SendInternal($"FileSystemInfo.get_FileSystemObject : fullPath = {aThis.FullName}");
            if (aStorage == null || aStorage.mFullPath != aThis.FullName)
            {
                if (aThis is DirectoryInfo)
                {
                    aStorage = VFSManager.GetDirectory(aThis.FullName);
                }
                else if (aThis is FileInfo)
                {
                    aStorage = VFSManager.GetFile(aThis.FullName);
                }
            }

            return aThis;
        }
    }
}
