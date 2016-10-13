//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(DirectoryInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(DirectoryEntry))]
    [PlugField(FieldId = "$$FullPath$$", FieldType = typeof(string))]
    [PlugField(FieldId = "$$Name$$", FieldType = typeof(string))]
    public static class DirectoryInfoImpl
    {
        public static void Ctor(
            DirectoryInfo aThis,
            string aPath,
            [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage,
            [FieldAccess(Name = "$$FullPath$$")] ref string aFullPath,
            [FieldAccess(Name = "$$Name$$")] ref string aName)
        {
            Global.mFileSystemDebugger.SendInternal("DirectoryInfo.ctor:");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            aStorage = VFSManager.GetDirectory(aPath);
            aFullPath = aPath;
            aName = Path.GetFileName(aPath);
        }

        public static string get_Name(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.get_Name : Name = {aThis}");
            return aThis.ToString();
        }

        public static DirectoryInfo get_Parent(DirectoryInfo aThis, [FieldAccess(Name = "$$FullPath")] ref string aFullPath)
        {
            Global.mFileSystemDebugger.SendInternal("DirectoryInfo.get_Parent");
            var xParent = Directory.GetParent(aFullPath);
            return xParent;
        }

        public static DirectoryInfo get_Root(DirectoryInfo aThis, [FieldAccess(Name = "$$FullPath")] ref string aFullPath)
        {
            Global.mFileSystemDebugger.SendInternal("DirectoryInfo.get_Root");
            string xRootPath = Path.GetPathRoot(aFullPath);
            var xRoot = new DirectoryInfo(xRootPath);
            return xRoot;
        }

        public static bool get_Exists(DirectoryInfo aThis, [FieldAccess(Name = "$$FullPath$$")] ref string aFullPath)
        {
            Global.mFileSystemDebugger.SendInternal("DirectoryInfo.get_Exists");
            return VFSManager.DirectoryExists(aFullPath);
        }

        public static FileInfo[] GetFiles(DirectoryInfo aThis, string searchPattern, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            throw new NotImplementedException();
        }

        public static FileInfo[] GetFiles(DirectoryInfo aThis, string searchPattern, SearchOption searchOption, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
            {
                throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
            }

            throw new NotImplementedException();
        }

        public static FileInfo[] GetFiles(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            throw new NotImplementedException();
        }

        public static DirectoryInfo CreateSubdirectory(DirectoryInfo aThis, string path, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            throw new NotImplementedException();
        }

        public static string ToString([FieldAccess(Name = "$$Name$$")] ref string aName)
        {
            return aName;
        }

        //public static void Create()
        //{

        //}

        //public static DirectoryInfo[] GetDirectories()
        //{
        //    throw new NotImplementedException();
        //}

        //public static FileSystemInfo[] GetFileSystemInfos(String searchPattern)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static FileSystemInfo[] GetFileSystemInfos(String searchPattern, SearchOption searchOption)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }
        //    if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
        //    {
        //        throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static FileSystemInfo[] GetFileSystemInfos()
        //{
        //    throw new NotImplementedException();
        //}

        //public static DirectoryInfo[] GetDirectories(String searchPattern)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static DirectoryInfo[] GetDirectories(String searchPattern, SearchOption searchOption)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }
        //    if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
        //    {
        //        throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<DirectoryInfo> EnumerateDirectories()
        //{
        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<DirectoryInfo> EnumerateDirectories(String searchPattern)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<DirectoryInfo> EnumerateDirectories(String searchPattern, SearchOption searchOption)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }
        //    if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
        //    {
        //        throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileInfo> EnumerateFiles()
        //{
        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileInfo> EnumerateFiles(String searchPattern)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileInfo> EnumerateFiles(String searchPattern, SearchOption searchOption)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }
        //    if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
        //    {
        //        throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
        //{
        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(String searchPattern)
        //{
        //    if (searchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string aSearchPattern, SearchOption aSearchOption)
        //{
        //    if (aSearchPattern == null)
        //    {
        //        throw new ArgumentNullException("searchPattern");
        //    }
        //    if ((aSearchOption != SearchOption.TopDirectoryOnly) && (aSearchOption != SearchOption.AllDirectories))
        //    {
        //        throw new ArgumentOutOfRangeException("searchOption", "Argument is out of range.");
        //    }

        //    throw new NotImplementedException();
        //}

        //public static void MoveTo(String destDirName)
        //{
        //    if (destDirName == null)
        //    {
        //        throw new ArgumentNullException("destDirName");
        //    }
        //    if (destDirName.Length == 0)
        //    {
        //        throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destDirName");
        //    }

        //    String fullDestDirName = Path.GetFullPathInternal(destDirName);
        //    if (!fullDestDirName.EndsWith(Path.DirectorySeparatorChar))
        //        fullDestDirName = fullDestDirName + Path.DirectorySeparatorChar;

        //    String fullSourcePath;
        //    if (FullPath.EndsWith(Path.DirectorySeparatorChar))
        //        fullSourcePath = FullPath;
        //    else
        //        fullSourcePath = FullPath + Path.DirectorySeparatorChar;

        //    if (String.Compare(fullSourcePath, fullDestDirName, StringComparison.OrdinalIgnoreCase) == 0)
        //        throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));

        //    String sourceRoot = Path.GetPathRoot(fullSourcePath);
        //    String destinationRoot = Path.GetPathRoot(fullDestDirName);

        //    if (String.Compare(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase) != 0)
        //        throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));

        //    // TODO: Do the move


        //    FullPath = fullDestDirName;
        //    OriginalPath = destDirName;
        //    DisplayPath = GetDisplayName(OriginalPath, FullPath);
        //}

        //public static void Delete()
        //{
        //    throw new NotImplementedException();
        //    //Directory.Delete(FullPath, OriginalPath, false, true);
        //}

        //public static void Delete(bool recursive)
        //{
        //    throw new NotImplementedException();
        //    //Directory.Delete(FullPath, OriginalPath, recursive, true);
        //}

    }
}
