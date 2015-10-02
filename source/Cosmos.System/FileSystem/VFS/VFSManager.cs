using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.FileSystem.VFS
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
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r',
                ':',
                '*',
                '?',
                '\\',
                '/'
            };
        }

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
            return new[]
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r',
                '*',
                '?'
            };
        }

        public static char GetPathSeparator()
        {
            return ';';
        }

        public static char[] GetRealInvalidPathChars()
        {
            return new[]
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r'
            };
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

        public static Listing.File  TryGetFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }
            FatHelpers.Debug("In VFSManager.TryGetFile");
            string xFileName = Path.GetFileName(aPath);
            string xDirectory = Path.GetDirectoryName(aPath);
            var xLastChar = xDirectory[xDirectory.Length - 1];
            if (xLastChar != Path.DirectorySeparatorChar)
            {
                xDirectory = xDirectory + Path.DirectorySeparatorChar;
            }
            FatHelpers.Debug("Now Try to get directory listing");
            var xList = GetDirectoryListing(xDirectory);
            for (int i = 0; i < xList.Count; i++)
            {
                var xEntry = xList[i];
                var xFile = xEntry as Listing.File;
                if (xFile != null && String.Equals(xEntry.Name, xFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return xFile;
                }
            }

            return null;
        }

        public static List<Listing.File> GetFiles(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
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

        public static Listing.Directory GetDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }
            FatHelpers.Debug("In VFSManager.GetDirectory");

            return mVFS.GetDirectory(aPath);
        }

        public static List<Listing.Base> GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return mVFS.GetDirectoryListing(aPath);
        }

        public static Listing.Directory GetVolume(string aVolume)
        {
            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException("aVolume");
            }

            return null;
        }

        public static List<Listing.Directory> GetVolumes()
        {
            return null;
        }

        public static List<string> GetLogicalDrives()
        {
            //TODO: Directory.GetLogicalDrives() will call this.
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
                FatHelpers.Debug("In VFSManager.FileExists");
                return (VFSManager.TryGetFile(aPath) != null);
            }
            catch (Exception E)
            {
                // don't ever remove this method, even if it doesn't contain any code!
                FatHelpers.Debug("Exception occurred in VFSManager.FileExists: ");
                // don't ever remove this method, even if it doesn't contain any code!
                FatHelpers.Debug(E.Message);
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            try
            {
                FatHelpers.Debug("DirectoryExists. Path = '" + aPath + "'");
                string xDir = string.Concat(aPath, VFSBase.DirectorySeparatorChar);
                //xDir = Path.GetDirectoryName(xDir);
                FatHelpers.Debug("Before VFSManager.GetDirectory");
                return (VFSManager.GetDirectory(xDir) != null);
            }
            catch (Exception E)
            {
                FatHelpers.Debug("Exception occurred in VFSManager.DirectoryExists: ");
                FatHelpers.Debug(E.Message);
                return false;
            }

        }

        public static Stream GetFileStream(string aPathname)
        {
            var xFileInfo = TryGetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.FileSystem.GetFileStream(xFileInfo);
        }
    }
}
