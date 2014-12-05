using System;
using System.Collections.Generic;
using System.IO;

namespace SentinelKernel.System.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase mVFS;

        public static void RegisterVFS(VFSBase aVFS)
        {
           Cosmos.System.Global.Dbg.Send("VFSManager.RegisterVFS");
            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        #region Helpers

        public static bool IsAbsolutePath(this string aPath)
        {
            return ((aPath[0] == VFSBase.DirectorySeparatorChar) || (aPath[0] == VFSBase.AltDirectorySeparatorChar));
        }

        public static bool IsRelativePath(this string aPath)
        {
            return (!(aPath[0] == VFSBase.DirectorySeparatorChar) || !(aPath[0] == VFSBase.AltDirectorySeparatorChar));
        }

        public static string[] SplitPath(string aPath)
        {
            return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
        }

        private static char[] GetDirectorySeparators()
        {
            return new char[] { VFSBase.DirectorySeparatorChar, VFSBase.AltDirectorySeparatorChar };
        }

        #endregion

        public static System.FileSystem.Listing.File GetFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return null;

            /*
            string xFileName = Path.GetFileName(aPath);
            string xDirectory = Path.GetDirectoryName(aPath) + Path.DirectorySeparatorChar;

            foreach (var xEntry in GetDirectoryListing(xDirectory))
            {
                if ((xEntry is FileSystem.Listing.File) && (xEntry.Name == xFileName))
                {
                    return (xEntry as FileSystem.Listing.File);
                }
            }

            return null;
            */ 
        }

        public static List<System.FileSystem.Listing.File> GetFiles(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("sPath");
            }

            return null;

            /*
            List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            var xDirName = Path.GetDirectoryName(aPath);
            var xEntries = GetDirectoryListing(xDirName);

            for (int i = 0; i < xEntries.Length; i++)
            {
                var entry = xEntries[i];
                if (!entry.IsDirectory)
                    xFiles.Add(entry);
            }

            return xFiles.ToArray();
            */ 
        }

        public static System.FileSystem.Listing.Directory GetDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return mVFS.GetDirectory(aPath);
        }

        public static List<System.FileSystem.Listing.Base> GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return mVFS.GetDirectoryListing(aPath);
        }

        public static System.FileSystem.Listing.Directory GetVolume(string aVolume)
        {
            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException("aVolume");
            }

            return null;
        }

        public static List<System.FileSystem.Listing.Directory> GetVolumes()
        {
            return null;
        }

        public static List<string> GetLogicalDrives()
        {
            return null;

            /*
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetVolumes())
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
            */ 
        }

        public static List<string> InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
            return null;

            /*
            //TODO: Add SearchOption functionality
            //TODO: What is userPathOriginal?
            //TODO: Add SearchPattern functionality

            List<string> xFileAndDirectoryNames = new List<string>();

            //Validate input arguments
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
                throw new ArgumentOutOfRangeException("searchOption");

            searchPattern = searchPattern.TrimEnd(new char[0]);
            if (searchPattern.Length == 0)
                return new string[0];

            //Perform search in filesystem
            FilesystemEntry[] xEntries = GetDirectoryListing(path);

            foreach (FilesystemEntry xEntry in xEntries)
            {
                if (xEntry.IsDirectory && includeDirs)
                    xFileAndDirectoryNames.Add(xEntry.Name);
                else if (!xEntry.IsDirectory && includeFiles)
                    xFileAndDirectoryNames.Add(xEntry.Name);
            }

            return xFileAndDirectoryNames.ToArray();
            
             */ 
        }

        public static bool FileExists(string aPath)
        {
            try
            {
                return (VFSManager.GetFile(aPath) != null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            try
            {
                string xDir = aPath + VFSBase.DirectorySeparatorChar;
                xDir = Path.GetDirectoryName(xDir);
                return (VFSManager.GetDirectory(xDir) != null);
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}