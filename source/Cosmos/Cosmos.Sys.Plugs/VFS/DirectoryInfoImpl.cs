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
    //[PlugField(FieldId = "$$Path$$", FieldType = typeof(String))]
    public static class DirectoryInfoImpl
    {
        [PlugMethod(Signature="System_Void__System_IO_DirectoryInfo__ctor_System_String_")]
        public static void ctor(
            DirectoryInfo aThis,
            [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage,
            String aPath
            )
        {
            if (aPath == null)
                throw new ArgumentNullException("aPath is null in DirectoryInfo ctor");

            //Search for directory
            if (!VFSManager.DirectoryExists(aPath))
                throw new DirectoryNotFoundException("Unable to find directory " + aPath);

            //If it exists, then get the directory as a FilesystemEntry
            aStorage = VFSManager.GetDirectoryEntry(aPath);
        }

        public static bool get_Exists([FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            //TODO: actually test if it exists
            return (aStorage != null);
        }

        public static string get_FullName([FieldAccess(Name="$$Storage$$")] ref FilesystemEntry aStorage)
        {
            //TODO: return FULL name
            return aStorage.Name;
        }

        public static string get_Name([FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            return aStorage.Name;
        }

        //public static string ToString([FieldAccess(Name = "$$Path$$")] ref String aPath)
        //{
        //    return "ToString()";
        //}
    }
}
