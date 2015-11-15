using System.IO;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    using Cosmos.System.FileSystem;

    using global::System;
    using global::System.Collections.Generic;

    [Plug(Target = typeof(DirectoryInfo))]
    [PlugField(FieldId = "$$Storage$$", FieldType = typeof(DirectoryEntry))]
    [PlugField(FieldId = "$$FullPath$$", FieldType = typeof(string))]
    public static class DirectoryInfoImpl
    {
        public static void Ctor(DirectoryInfo aThis, string aPath,
            [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage,
            [FieldAccess(Name = "$$FullPath$$")] ref string aFullPath)
        {
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath is null in DirectoryInfo ctor");
            }

            if (!VFSManager.DirectoryExists(aPath))
            {
                throw new DirectoryNotFoundException("Unable to find directory " + aPath);
            }

            aStorage = VFSManager.GetDirectory(aPath);
            aFullPath = aPath;
        }

        public static string get_Name(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            FatHelpers.Debug("-- DirectoryInfo.get_Name --");
            throw new NotImplementedException();
        }

        public static DirectoryInfo get_Parent(DirectoryInfo aThis)
        {
            FatHelpers.Debug("-- DirectoryInfo.get_Parent --");
            throw new NotImplementedException();
        }

        public static DirectoryInfo get_Root(DirectoryInfo aThis)
        {
            FatHelpers.Debug("-- DirectoryInfo.get_Root --");
            throw new NotImplementedException();
        }

        public static bool get_Exists(DirectoryInfo aThis, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            FatHelpers.Debug("-- DirectoryInfo.get_Exists --");
            return VFSManager.DirectoryExists(aStorage);
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
            return null;
        }

        public static DirectoryInfo CreateSubdirectory(DirectoryInfo aThis, string path, [FieldAccess(Name = "$$Storage$$")] ref DirectoryEntry aStorage)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            throw new NotImplementedException();
        }

        public static string ToString([FieldAccess(Name = "$$Path$$")] ref string aPath)
        {
            return "DirectoryInfo.ToString() not yet implemented";
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

