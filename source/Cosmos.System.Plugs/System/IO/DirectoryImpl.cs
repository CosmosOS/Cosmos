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
            FatHelpers.Debug("-- Directory.GetCurrentDirectory --");
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aPath)
        {
            FatHelpers.Debug("-- Directory.SetCurrentDirectory --");
            mCurrentDirectory = aPath;
        }

        public static bool Exists(string aPath)
        {
            FatHelpers.Debug("-- Directory.Exists --");
            return VFSManager.DirectoryExists(aPath);
        }

        public static DirectoryInfo CreateDirectory(string aPath)
        {
            FatHelpers.Debug("-- Directory.CreateDirectory --");
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            var xEntry = VFSManager.CreateDirectory(aPath);
            if (xEntry == null)
            {
                return null;
            }

            return new DirectoryInfo(aPath);
        }

        public static DirectoryInfo GetParent(string aPath)
        {
            FatHelpers.Debug("-- Directory.GetParent --");
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            string xFullPath = Path.GetFullPath(aPath);
            string xName = Path.GetDirectoryName(xFullPath);
            if (xName == null)
            {
                return null;
            }

            return new DirectoryInfo(xFullPath);
        }

        public static string[] GetDirectories(string aPath)
        {
            FatHelpers.Debug("-- Directory.GetDirectories --");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xDirectories = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].EntryType == DirectoryEntryTypeEnum.Directory)
                {
                    xDirectories.Add(xEntries[i].Name);
                }
            }

            return xDirectories.ToArray();
        }

        public static string[] GetFiles(string aPath)
        {
            FatHelpers.Debug("-- Directory.GetFiles --");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xFiles = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].EntryType == DirectoryEntryTypeEnum.File)
                {
                    xFiles.Add(xEntries[i].Name);
                }
            }
            
            return xFiles.ToArray();
        }

    }
}