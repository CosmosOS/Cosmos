using GCImplementation = Cosmos.Core.GCImplementation;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        #region Methods

        public static string GetCurrentDirectory()
        {
            Global.FileSystemDebugger.SendInternal($"Directory.GetCurrentDirectory : currentDirectory = {currentDirectory}");
            return currentDirectory;
        }

        public static void SetCurrentDirectory(string path)
        {
            if (Path.IsPathRooted(path))
            {
                currentDirectory = string.Empty;
            }

            // Create as referencable variable to prevent memory leaks.
            string[] Sections = path.Split(Path.DirectorySeparatorChar);

            foreach (string Section in Sections)
            {
                currentDirectory = Section switch
                {
                    ".." => currentDirectory[..currentDirectory[..^1].LastIndexOf(Path.DirectorySeparatorChar)] + Path.DirectorySeparatorChar,
                    "." => string.Empty,
                    _ => currentDirectory + Section + (Section.EndsWith(Path.DirectorySeparatorChar) ? string.Empty : Path.DirectorySeparatorChar),
                };
            }

            currentDirectory = Path.GetFullPath(currentDirectory);

            // Free the temporary array.
            GCImplementation.Free(Sections);

            Global.FileSystemDebugger.SendInternal($"Directory.SetCurrentDirectory : path = {path}");
        }

        public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(path);

            Global.FileSystemDebugger.SendInternal($"Directory.Exists : path = {fullPath}");

            bool returnValue = VFSManager.DirectoryExists(fullPath);
            GCImplementation.Free(fullPath);
            return returnValue;
        }

        public static DirectoryInfo CreateDirectory(string path)
        {
            Global.FileSystemDebugger.SendInternal($"-- Directory.CreateDirectory --");
            Global.FileSystemDebugger.SendInternal($"path = {path}");

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            string fullPath = Path.GetFullPath(path);

            DirectoryEntry entry = VFSManager.CreateDirectory(fullPath);

            if (entry == null)
            {
                return null;
            }

            GCImplementation.Free(entry);

            return new DirectoryInfo(fullPath);
        }

        public static void Delete(string path)
        {
            string fullPath = Path.GetFullPath(path);
            Delete(fullPath, false);
            GCImplementation.Free(fullPath);
        }

        public static void Delete(string path, bool recursive)
        {
            string fullPath = Path.GetFullPath(path);
            VFSManager.DeleteDirectory(fullPath, recursive);
            GCImplementation.Free(fullPath);
        }

        public static DirectoryInfo GetParent(string path)
        {
            Global.FileSystemDebugger.SendInternal("Directory.GetParent:");

            if (string.IsNullOrEmpty(path))
            {
                Global.FileSystemDebugger.SendInternal("Directory.GetParent : path is empty");
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            Global.FileSystemDebugger.SendInternal($"path = {path}");

            string fullPath = Path.GetFullPath(path);
            string parentDirectory = Path.GetDirectoryName(fullPath);
            GCImplementation.Free(fullPath);

            if (parentDirectory == null)
            {
                Global.FileSystemDebugger.SendInternal("Directory.GetParent : Parent Directory is null");
                return null;
            }

            return new DirectoryInfo(parentDirectory);
        }

        public static string[] GetDirectories(string path)
        {
            Global.FileSystemDebugger.SendInternal("Directory.GetDirectories");

            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            string fullPath = Path.GetFullPath(path);

            List<string> directories = new();
            List<DirectoryEntry> entries = VFSManager.GetDirectoryListing(fullPath);
            GCImplementation.Free(fullPath);

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
            Global.FileSystemDebugger.SendInternal("Directory.GetFiles");

            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            string fullPath = Path.GetFullPath(path);

            List<string> files = new();
            List<DirectoryEntry> entries = VFSManager.GetDirectoryListing(fullPath);
            GCImplementation.Free(fullPath);

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

        private static string currentDirectory = "0:\\";

        #endregion
    }
}