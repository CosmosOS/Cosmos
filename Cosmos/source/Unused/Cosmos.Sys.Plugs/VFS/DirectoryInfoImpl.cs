using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using System.IO;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.Plugs
{
    [Plug(Target=typeof(DirectoryInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(FilesystemEntry))]
    [PlugField(FieldId = "$$FullPath$$", FieldType = typeof(String))]
    public static class DirectoryInfoImpl
    {
        [PlugMethod(/*Signature = "System_Void__System_IO_DirectoryInfo__ctor_System_String_"*/)]
        public static void Ctor(
            DirectoryInfo aThis,
            //[FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage,
            //[FieldAccess(Name = "$$FullPath$$")] String aFullPath,
            String aPath
            )
        {
            //if (aPath == null)
            //    throw new ArgumentNullException("aPath is null in DirectoryInfo ctor");

            ////Search for directory
            ////if (!VFSManager.DirectoryExists(aPath))
            ////    throw new DirectoryNotFoundException("Unable to find directory " + aPath);

            ////If it exists, then get the directory as a FilesystemEntry
            //aStorage = VFSManager.GetDirectoryEntry(aPath);
            //aFullPath = aPath;
        }

        public static bool get_Exists(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            //TODO: actually test if it exists
            return (aStorage != null);
        }

        //public static string FullName
        //{
        //    get
        //    {
        //        return ".FullName isn't implemented yet";
        //    }
        //}

        //public static string get_FullName(DirectoryInfo aThis, [FieldAccess(Name = "$$FullPath$$")] String aFullPath)
        //{
        //    //TODO: return FULL name
        //    return aFullPath;
        //}

        public static string get_Name(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            return aStorage.Name;
        }

        public static FileInfo[] GetFiles(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref FilesystemEntry aStorage)
        {
            List<FileInfo> xFiles = new List<FileInfo>();
            var xEntries = VFSManager.GetFiles(aStorage);

            foreach (FilesystemEntry xEntry in xEntries)
            {
                xFiles.Add(new FileInfo(xEntry.Name));
            }

            return xFiles.ToArray();

            //Alternative implementation
            //var xEntries = VFSManager.GetFiles(aStorage);
            //FileInfo[] files = new FileInfo[xEntries.Length];
            //for (int i = 0; i < xEntries.Length; i++)
            //{
            //    files[i] = new FileInfo(xEntries[i].Name);
            //}

            //return files;
        }


        public static string ToString([FieldAccess(Name = "$$Path$$")] ref String aPath)
        {
            return "DirectoryInfo.ToString() not yet implemented";
        }
    }
}
