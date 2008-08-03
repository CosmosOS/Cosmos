using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.IO;
using Cosmos.FileSystem;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target = typeof(FileInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(FilesystemEntry))]
    public static class FileInfoImpl
    {
        [PlugMethod(Signature = "System_Void__System_IO_FileInfo__ctor_System_String_")]
        public static void ctor(
            FileInfo aThis,
            [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage,
            String aFile
            )
        {
            var xEntry = VFSManager.GetDirectoryEntry(aFile);

            if (!xEntry.IsDirectory)
                aStorage = xEntry;
        }

        public static string get_Name([FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            return "Filename" + aStorage.Name;
        }

        public static bool get_Exists([FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            return VFSManager.FileExists(aStorage.Name);
        }
    }
}
