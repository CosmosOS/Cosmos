namespace Cosmos.System.FileSystem.VFS
{
    using Cosmos.System.FileSystem.Listing;

    using global::System;
    using global::System.Collections.Generic;
    using global::System.IO;

    public static class VFSManager
    {
        private static VFSBase mVFS;

        public static void RegisterVFS(VFSBase aVFS)
        {
            //FatHelpers.Debug("-- VFSManager.RegisterVFS --");

            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        public static DirectoryEntry GetFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            //FatHelpers.Debug("-- VFSManager.GetFile : aPath = " + aPath + " --");

            string xFileName = Path.GetFileName(aPath);
            string xDirectory = Path.GetDirectoryName(aPath);
            char xLastChar = xDirectory[xDirectory.Length - 1];
            if (xLastChar != Path.DirectorySeparatorChar)
            {
                xDirectory = xDirectory + Path.DirectorySeparatorChar;
            }

            var xList = GetDirectoryListing(xDirectory);
            for (int i = 0; i < xList.Count; i++)
            {
                var xEntry = xList[i];
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.File)
                    && (xEntry.mName.ToUpper() == xFileName.ToUpper()))
                {
                    return xEntry;
                }
            }

            return null;
        }

        public static DirectoryEntry CreateDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            //FatHelpers.Debug("-- VFSManager.CreateDirectory : aPath = " + aPath + " --");
            return mVFS.CreateDirectory(aPath);
        }

        public static DirectoryEntry GetDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            //FatHelpers.Debug("-- VFSManager.GetDirectory : aPath = " + aPath + " --");
            return mVFS.GetDirectory(aPath);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            //FatHelpers.Debug("-- VFSManager.GetDirectoryListing : aPath = " + aPath + " --");
            return mVFS.GetDirectoryListing(aPath);
        }

        public static DirectoryEntry GetVolume(string aVolume)
        {
            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException("aVolume");
            }

            //FatHelpers.Debug("-- VFSManager.GetVolume : aVolume = " + aVolume + " --");

            return mVFS.GetVolume(aVolume);
        }

        public static List<DirectoryEntry> GetVolumes()
        {
            //FatHelpers.Debug("-- VFSManager.GetVolumes --");

            return mVFS.GetVolumes();
        }

        public static List<string> GetLogicalDrives()
        {
            //FatHelpers.Debug("-- VFSManager.GetLogicalDrives --");

            //TODO: Directory.GetLogicalDrives() will call this.
            return null;

            /*
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetVolumes())
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
            */
        }

        public static List<string> InternalGetFileDirectoryNames(
            string path,
            string userPathOriginal,
            string searchPattern,
            bool includeFiles,
            bool includeDirs,
            SearchOption searchOption)
        {
            //FatHelpers.Debug("-- VFSManager.InternalGetFileDirectoryNames --");

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
            if (aPath == null)
            {
                return false;
            }

            try
            {
                //FatHelpers.Debug("-- VFSManager.FileExists : aPath = " + aPath + " --");

                string xPath = Path.GetFullPath(aPath);
                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                Console.Write("Exception occurred: ");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool FileExists(DirectoryEntry aEntry)
        {
            //FatHelpers.Debug("-- VFSManager.FileExists --");

            try
            {
                string xPath = GetFullPath(aEntry);
                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                Console.Write("Exception occurred: ");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            //FatHelpers.Debug("-- VFSManager.DirectoryExists --");

            try
            {
                string xPath = Path.GetFullPath(aPath);
                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                Console.Write("Exception occurred: ");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(DirectoryEntry aEntry)
        {
            //FatHelpers.Debug("-- VFSManager.DirectoryExists --");

            try
            {
                string xPath = GetFullPath(aEntry);
                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                Console.Write("Exception occurred: ");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static string GetFullPath(DirectoryEntry aEntry)
        {
            var xEntry = aEntry;
            string xPath = string.Empty;

            while (xEntry != null)
            {
                xPath = xPath + xEntry.mName;
                xEntry = aEntry.mParent;
                //FatHelpers.Debug("-- VFSManager.GetFullPath : xPath = " + xPath + " --");
            }

            //FatHelpers.Debug("-- VFSManager.GetFullPath : result = " + xPath + " --");
            return xPath;
        }

        public static Stream GetFileStream(string aPathname)
        {
            //FatHelpers.Debug("-- VFSManager.GetFileStream --");

            var xFileInfo = GetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.GetFileStream();
        }

        #region Helpers

        public static char GetAltDirectorySeparatorChar()
        {
            return '/';
        }

        public static char GetDirectorySeparatorChar()
        {
            return '\\';
        }

        public static char[] GetInvalidFileNameChars()
        {
            return new[]
                       { '"', '<', '>', '|', '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', ':', '*', '?', '\\', '/' };
        }

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
            return new[] { '"', '<', '>', '|', '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '*', '?' };
        }

        public static char GetPathSeparator()
        {
            return ';';
        }

        public static char[] GetRealInvalidPathChars()
        {
            return new[] { '"', '<', '>', '|', '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r' };
        }

        public static char[] GetTrimEndChars()
        {
            return new[] { (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85, (char)0xA0 };
        }

        public static char GetVolumeSeparatorChar()
        {
            return ':';
        }

        public static int GetMaxPath()
        {
            return 260;
        }

        //public static bool IsAbsolutePath(this string aPath)
        //{
        //    return ((aPath[0] == VFSBase.DirectorySeparatorChar) || (aPath[0] == VFSBase.AltDirectorySeparatorChar));
        //}

        //public static bool IsRelativePath(this string aPath)
        //{
        //    return (aPath[0] != VFSBase.DirectorySeparatorChar || aPath[0] != VFSBase.AltDirectorySeparatorChar);
        //}

        public static string[] SplitPath(string aPath)
        {
            //TODO: This should call Path.GetDirectoryName() and then loop calling Directory.GetParent(), but those aren't implemented yet.
            return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
        }

        private static char[] GetDirectorySeparators()
        {
            return new[] { GetDirectorySeparatorChar(), GetAltDirectorySeparatorChar() };
        }

        #endregion
    }
}