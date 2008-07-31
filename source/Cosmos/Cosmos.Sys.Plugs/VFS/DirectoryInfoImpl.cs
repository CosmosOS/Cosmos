using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.IO;
using Cosmos.FileSystem;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target=typeof(DirectoryInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(FilesystemEntry))]
    public class DirectoryInfoImpl
    {

        //NB; THIS PLUG DOESN'T WORK YET!

        [PlugMethod(Signature="System_Void__System_IO_DirectoryInfo__ctor_System_String_")]
        public static void ctor(
            DirectoryInfo aThis,
            [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage,
            String aPath
            )
        {
            //Search for directory
            if (!VFSManager.DirectoryExists(aPath))
                throw new DirectoryNotFoundException("Unable to find directory " + aPath);

            //If it exists, then get the directory as a FilesystemEntry
            aStorage = VFSManager.GetDirectoryEntry(aPath);
        }


        //Plug for DirectoryInfo.Fullname
        public string get_FullName([FieldAccess(Name="$$Storage$$")] ref FilesystemEntry aStorage)
        {
            return aStorage.Name;
        }
    }
}
