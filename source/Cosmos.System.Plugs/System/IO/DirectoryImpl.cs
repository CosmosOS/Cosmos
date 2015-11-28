using System;
using System.IO;
using System.Collections.Generic;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        private static string mCurrentDirectory = string.Empty;

        public static string GetCurrentDirectory()
        {
            FileSystemHelpers.Debug("Directory.GetCurrentDirectory", "mCurrentDirectory =", mCurrentDirectory);
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aPath)
        {
            FileSystemHelpers.Debug("Directory.SetCurrentDirectory", "aPath =", aPath);
            mCurrentDirectory = aPath;
        }

        public static bool Exists(string aPath)
        {
            if (aPath == null)
            {
                return false;
            }

            FileSystemHelpers.Debug("Directory.Exists", "aPath =", aPath);
            return VFSManager.DirectoryExists(aPath);
        }

        public static DirectoryInfo CreateDirectory(string aPath)
        {
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            FileSystemHelpers.Debug("Directory.CreateDirectory", "aPath =", aPath);
            var xEntry = VFSManager.CreateDirectory(aPath);
            if (xEntry == null)
            {
                return null;
            }

            return new DirectoryInfo(aPath);
        }

        public static DirectoryInfo GetParent(string aPath)
        {
            if (aPath == null)
            {
                FileSystemHelpers.Debug("Directory.GetParent", "aPath is null");
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                FileSystemHelpers.Debug("Directory.GetParent", "aPath is empty");
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            string xFullPath = Path.GetFullPath(aPath);
            string xParentDirectory = Path.GetDirectoryName(xFullPath);
            if (xParentDirectory == null)
            {
                FileSystemHelpers.Debug("Directory.GetParent", "xParentDirectory is null");
                return null;
            }
            
            return new DirectoryInfo(xParentDirectory);
        }

        public static string[] GetDirectories(string aPath)
        {
            FileSystemHelpers.Debug("Directory.GetDirectories");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xDirectories = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    xDirectories.Add(xEntries[i].mName);
                }
            }

            return xDirectories.ToArray();
        }

        public static string[] GetFiles(string aPath)
        {
            FileSystemHelpers.Debug("Directory.GetFiles");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xFiles = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    xFiles.Add(xEntries[i].mName);
                }
            }
            
            return xFiles.ToArray();
        }

    }
}