using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System;
using IL2CPU.API.Attribs;
using IL2CPU.API;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        #region Methods

        public static string GetCurrentDirectory()
        {
            Global.Debugger.SendInternal($"Directory.GetCurrentDirectory : currentDirectory = {currentDirectory}");
            return currentDirectory;
        }

        public static void SetCurrentDirectory(string path)
        {
            Global.Debugger.SendInternal($"Directory.SetCurrentDirectory : path = {path}");
            currentDirectory = path;
        }

        public static bool Exists(string path)
        {
            if (path == null)
            {
                return false;
            }

            Global.Debugger.SendInternal($"Directory.Exists : aPath = {path}");
            return VFSManager.DirectoryExists(path);
        }

        public static DirectoryInfo CreateDirectory(string path)
        {
            Global.Debugger.SendInternal($"-- Directory.CreateDirectory --");
            Global.Debugger.SendInternal($"path = {path}");

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            DirectoryEntry entry = VFSManager.CreateDirectory(path);

            if (entry == null)
            {
                return null;
            }

            return new DirectoryInfo(path);
        }

        public static void Delete(string aPath)
        {
            Delete(aPath, false);
        }

        public static void Delete(string path, bool recursive)
        {
            string fullPath = Path.GetFullPath(path);

            VFSManager.DeleteDirectory(fullPath, recursive);
        }

        public static DirectoryInfo GetParent(string path)
        {
            Global.Debugger.SendInternal("Directory.GetParent:");

            if (path == null)
            {
                Global.Debugger.SendInternal("Directory.GetParent : path is null");
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                Global.Debugger.SendInternal("Directory.GetParent : path is empty");
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            Global.Debugger.SendInternal($"path = {path}");

            string fullPath = Path.GetFullPath(path);
            string parentDirectory = Path.GetDirectoryName(fullPath);
            if (parentDirectory == null)
            {
                Global.Debugger.SendInternal("Directory.GetParent : Parent Directory is null");
                return null;
            }

            return new DirectoryInfo(parentDirectory);
        }

        public static string[] GetDirectories(string path)
        {
            Global.Debugger.SendInternal("Directory.GetDirectories");
            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            List<string> directories = new();
            List<DirectoryEntry> entries = VFSManager.GetDirectoryListing(path);

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    directories.Add(entries[i].mName);
                }
            }

            return directories.ToArray();
        }

        public static string[] GetFiles(string path)
        {
            Global.Debugger.SendInternal("Directory.GetFiles");
            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            List<string> files = new();
            List<DirectoryEntry> entries = VFSManager.GetDirectoryListing(path);

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    files.Add(entries[i].mName);
                }
            }

            return files.ToArray();
        }

        #endregion

        #region Fields

        private static string currentDirectory = string.Empty;

        #endregion
    }
}