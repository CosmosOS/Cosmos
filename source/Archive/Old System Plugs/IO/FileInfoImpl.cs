using global::System.IO;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;

//using Directory = Cosmos.System.FileSystem.Listing.Directory;
//using File = Cosmos.System.FileSystem.Listing.File;

namespace Cosmos.System.Plugs.System.IO
{
    

    [Plug(Target = typeof(FileInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(DirectoryEntry))]
    public static class FileInfoImpl
    {
        //[PlugMethod(Signature = "System_Void__System_IO_FileInfo__ctor_System_String_")]
        public static void Ctor(FileInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage, string aFile)
        {
            /*
            //Determine if aFile is relative or absolute
            string xFile;
            if (aFile.IsRelativePath())
                xFile = Directory.GetCurrentDirectory() + aFile;
            else
                xFile = aFile;

            var xEntry = VFSManager.GetDirectory(xFile);

            if (xEntry is FileSystem.Listing.Directory)
            {
                aStorage = xEntry as FileSystem.Listing.Directory;
            }
            */
        }

        public static string get_Name([FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            return "Filename" + aStorage.mName;
        }

        public static bool get_Exists([FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            return VFSManager.FileExists(aStorage.mName);
        }

        public static string ToString(FileInfo aThis)
        {
            return "FileInfo.ToString() not yet implemented!";
        }
    }
}
