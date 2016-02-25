using Cosmos.Debug.Kernel;

using global::System;
using global::System.Collections.Generic;
using global::System.IO;

using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase mVFS;

        public static void RegisterVFS(VFSBase aVFS)
        {
            Global.mFileSystemDebugger.SendInternal("VFSManager.RegisterVFS");
            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        public static DirectoryEntry CreateFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.CreateFile : aPath = {aPath}");

            return mVFS.CreateFile(aPath);
        }

        public static DirectoryEntry GetFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetFile : aPath = {aPath}");

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

            Global.mFileSystemDebugger.SendInternal($"VFSManager.CreateDirectory : aPath = {aPath}");

            return mVFS.CreateDirectory(aPath);
        }

        public static DirectoryEntry GetDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetDirectory : aPath = {aPath}");

            return mVFS.GetDirectory(aPath);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetDirectoryListing : aPath = {aPath}");

            return mVFS.GetDirectoryListing(aPath);
        }

        public static DirectoryEntry GetVolume(string aVolume)
        {
            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException("aVolume");
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetVolume : aVolume = {aVolume}");

            return mVFS.GetVolume(aVolume);
        }

        public static List<DirectoryEntry> GetVolumes()
        {
            Global.mFileSystemDebugger.SendInternal("VFSManager.GetVolumes");

            return mVFS.GetVolumes();
        }

        public static List<string> GetLogicalDrives()
        {
            Global.mFileSystemDebugger.SendInternal("VFSManager.GetLogicalDrives");

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
            Global.mFileSystemDebugger.SendInternal("VFSManager.InternalGetFileDirectoryNames");

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
                Global.mFileSystemDebugger.SendInternal($"VFSManager.FileExists : aPath = {aPath}");

                string xPath = Path.GetFullPath(aPath);
                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool FileExists(DirectoryEntry aEntry)
        {
            try
            {
                Global.mFileSystemDebugger.SendInternal($"VFSManager.FileExists : aEntry.mName = {aEntry?.mName}");
                string xPath = GetFullPath(aEntry);
                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            try
            {
                Global.mFileSystemDebugger.SendInternal($"VFSManager.DirectoryExists : aPath = {aPath}");
                string xPath = Path.GetFullPath(aPath);
                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(DirectoryEntry aEntry)
        {
            try
            {
                Global.mFileSystemDebugger.SendInternal($"VFSManager.DirectoryExists : aEntry.mName = {aEntry?.mName}");
                string xPath = GetFullPath(aEntry);
                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static string GetFullPath(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetFullPath : aEntry.mName = " + aEntry?.mName);
            var xParent = aEntry?.mParent;
            string xPath = aEntry?.mName;

            while (xParent != null)
            {
                xPath = xParent.mName + xPath;
                Global.mFileSystemDebugger.SendInternal($"VFSManager.GetFullPath : xPath = " + xPath);
                xParent = xParent.mParent;
            }

            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetFullPath : xPath = " + xPath);
            return xPath;
        }

        public static Stream GetFileStream(string aPathname)
        {
            Global.mFileSystemDebugger.SendInternal($"VFSManager.GetFileStream : aPathname = {aPathname}");
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
            char[] xReturn = new char[17];
            xReturn[0] = '"';
            xReturn[1] = '<';
            xReturn[2] = '>';
            xReturn[3] = '|';
            xReturn[4] = '\0';
            xReturn[5] = '\a';
            xReturn[6] = '\b';
            xReturn[7] = '\t';
            xReturn[8] = '\n';
            xReturn[9] = '\v';
            xReturn[10] = '\f';
            xReturn[11] = '\r';
            xReturn[12] = ':';
            xReturn[13] = '*';
            xReturn[14] = '?';
            xReturn[15] = '\\';
            xReturn[16] = '/';
            return xReturn;
        }

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
            char[] xReturn = new char[14];
            xReturn[0] = '"';
            xReturn[1] = '<';
            xReturn[2] = '>';
            xReturn[3] = '|';
            xReturn[4] = '\0';
            xReturn[5] = '\a';
            xReturn[6] = '\b';
            xReturn[7] = '\t';
            xReturn[8] = '\n';
            xReturn[9] = '\v';
            xReturn[10] = '\f';
            xReturn[11] = '\r';
            xReturn[12] = '*';
            xReturn[13] = '?';
            return xReturn;
        }

        public static char GetPathSeparator()
        {
            return ';';
        }

        public static char[] GetRealInvalidPathChars()
        {
            char[] xReturn = new char[12];
            xReturn[0] = '"';
            xReturn[1] = '<';
            xReturn[2] = '>';
            xReturn[3] = '|';
            xReturn[4] = '\0';
            xReturn[5] = '\a';
            xReturn[6] = '\b';
            xReturn[7] = '\t';
            xReturn[8] = '\n';
            xReturn[9] = '\v';
            xReturn[10] = '\f';
            xReturn[11] = '\r';
            return xReturn;
        }

        public static char[] GetTrimEndChars()
        {
            char[] xReturn = new char[8];
            xReturn[0] = (char)0x9;
            xReturn[1] = (char)0xA;
            xReturn[2] = (char)0xB;
            xReturn[3] = (char)0xC;
            xReturn[4] = (char)0xD;
            xReturn[5] = (char)0x20;
            xReturn[6] = (char)0x85;
            xReturn[7] = (char)0xA0;
            return xReturn;
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
