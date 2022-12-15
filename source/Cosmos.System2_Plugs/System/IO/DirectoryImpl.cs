//#define COSMOSDEBUG
using System;
using System.IO;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        private static string mCurrentDirectory = string.Empty;

        public static string GetCurrentDirectory()
        {
            Global.mFileSystemDebugger.SendInternal($"Directory.GetCurrentDirectory : mCurrentDirectory = {mCurrentDirectory}");
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal($"Directory.SetCurrentDirectory : aPath = {aPath}");
            mCurrentDirectory = aPath;
        }

        public static bool Exists(string aPath)
        {
            if (aPath == null)
            {
                return false;
            }

            Global.mFileSystemDebugger.SendInternal($"Directory.Exists : aPath = {aPath}");
            return VFSManager.DirectoryExists(aPath);
        }

        public static DirectoryInfo CreateDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal($"-- Directory.CreateDirectory --");
            Global.mFileSystemDebugger.SendInternal($"aPath = {aPath}");

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

        public static void Delete(string aPath)
        {
            Delete(aPath, false);
        }

        public static void Delete(string aPath, bool recursive)
        {
            String xFullPath = Path.GetFullPath(aPath);

            VFSManager.DeleteDirectory(xFullPath, recursive);
        }

        public static DirectoryInfo GetParent(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("Directory.GetParent:");

            if (aPath == null)
            {
                Global.mFileSystemDebugger.SendInternal("Directory.GetParent : aPath is null");
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                Global.mFileSystemDebugger.SendInternal("Directory.GetParent : aPath is empty");
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            string xFullPath = Path.GetFullPath(aPath);
            string xParentDirectory = Path.GetDirectoryName(xFullPath);
            if (xParentDirectory == null)
            {
                Global.mFileSystemDebugger.SendInternal("Directory.GetParent : xParentDirectory is null");
                return null;
            }

            return new DirectoryInfo(xParentDirectory);
        }

        public static string[] GetDirectories(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("Directory.GetDirectories");
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
            Global.mFileSystemDebugger.SendInternal("Directory.GetFiles");
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
