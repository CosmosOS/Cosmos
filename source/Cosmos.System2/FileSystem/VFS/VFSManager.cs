//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase mVFS;

        public static void RegisterVFS(VFSBase aVFS)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.RegisterVFS ---");
            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        public static DirectoryEntry CreateFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.CreateFile ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.CreateFile(aPath);
        }

        public static void DeleteFile(string aPath)
        {
            if (mVFS == null)
                throw new Exception("VFSManager isn't ready.");

            var xFile = mVFS.GetFile(aPath);

            if (xFile.mEntryType != DirectoryEntryTypeEnum.File)
                throw new UnauthorizedAccessException("The specified path isn't a file");

            mVFS.DeleteFile(xFile);
        }

        public static DirectoryEntry GetFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFile ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            string xFileName = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal("xFileName =");
            Global.mFileSystemDebugger.SendInternal(xFileName);

            string xDirectory = aPath.Remove(aPath.Length - xFileName.Length);
            Global.mFileSystemDebugger.SendInternal("xDirectory =");
            Global.mFileSystemDebugger.SendInternal(xDirectory);

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
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.CreateDirectory ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.CreateDirectory(aPath);
        }

        public static void DeleteDirectory(string aPath, bool recursive)
        {
            if (mVFS == null)
            {
                throw new Exception("VFSManager isn't ready.");
            }

            var xDirectory = mVFS.GetDirectory(aPath);
            var xDirectoryListing = mVFS.GetDirectoryListing(xDirectory);

            if (xDirectory.mEntryType != DirectoryEntryTypeEnum.Directory)
            {
                throw new IOException("The specified path isn't a directory");
            }

            if (xDirectoryListing.Count > 0 && !recursive)
            {
                throw new IOException("Directory is not empty");
            }

            if (recursive)
            {
                foreach (var entry in xDirectoryListing)
                {
                    if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                    {
                        DeleteDirectory(entry.mFullPath, true);
                    }
                    else if (entry.mEntryType == DirectoryEntryTypeEnum.File)
                    {
                        DeleteFile(entry.mFullPath);
                    }
                    else
                    {
                        throw new IOException("The directory contains a corrupted file");
                    }
                }
            }

            mVFS.DeleteDirectory(xDirectory);
        }

        public static DirectoryEntry GetDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectory ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.GetDirectory(aPath);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectoryListing ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.GetDirectoryListing(aPath);
        }

        public static DirectoryEntry GetVolume(string aVolume)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolume ---");

            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException(nameof(aVolume));
            }

            Global.mFileSystemDebugger.SendInternal("aVolume =");
            Global.mFileSystemDebugger.SendInternal(aVolume);

            return mVFS.GetVolume(aVolume);
        }

        public static List<DirectoryEntry> GetVolumes()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolumes ---");

            return mVFS.GetVolumes();
        }

        public static void RegisterFileSystem(FileSystemFactory aFileSystemFactory)
        {
            mVFS.RegisterFileSystem(aFileSystemFactory);
        }

        public static List<string> GetLogicalDrives()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetLogicalDrives ---");

            List<string> xDrives = new List<string>();
            foreach (DirectoryEntry entry in GetVolumes())
                xDrives.Add(entry.mName + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives;
        }

        public static List<string> InternalGetFileDirectoryNames(
            string path,
            string userPathOriginal,
            string searchPattern,
            bool includeFiles,
            bool includeDirs,
            SearchOption searchOption)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.InternalGetFileDirectoryNames ---");

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
            Global.mFileSystemDebugger.SendInternal("VFSManager.FileExists");

            if (aPath == null)
            {
                return false;
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aPath =");
                Global.mFileSystemDebugger.SendInternal(aPath);

                string xPath = Path.GetFullPath(aPath);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

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
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.FileExists ---");

            if (aEntry == null)
            {
                return false;
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aEntry.mName =");
                Global.mFileSystemDebugger.SendInternal(aEntry.mName);

                string xPath = GetFullPath(aEntry);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetFile(xPath) != null;
            }
            catch
            {
                /* Simply map any Exception to false as this method should return only bool */
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");

            try
            {
                Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

                string xPath = Path.GetFullPath(aPath);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetDirectory(xPath) != null;
            }
            catch
            {
                /* Simply map any Exception to false as this method should return only bool */
                return false;
            }
        }

        public static bool DirectoryExists(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");

            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aEntry.mName =");
                Global.mFileSystemDebugger.SendInternal(aEntry.mName);

                string xPath = GetFullPath(aEntry);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

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
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFullPath ---");

            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }

            Global.mFileSystemDebugger.SendInternal("aEntry.mName =");
            Global.mFileSystemDebugger.SendInternal(aEntry.mName);

            var xParent = aEntry.mParent;
            string xPath = aEntry.mName;

            while (xParent != null)
            {
                xPath = xParent.mName + xPath;
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                xParent = xParent.mParent;
                Global.mFileSystemDebugger.SendInternal("xParent.mName =");
                Global.mFileSystemDebugger.SendInternal(xParent.mName);
            }

            Global.mFileSystemDebugger.SendInternal("xPath =");
            Global.mFileSystemDebugger.SendInternal(xPath);

            return xPath;
        }

        public static Stream GetFileStream(string aPathname)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileStream ---");

            if (string.IsNullOrEmpty(aPathname))
            {
                return null;
            }

            Global.mFileSystemDebugger.SendInternal("aPathName =");
            Global.mFileSystemDebugger.SendInternal(aPathname);

            var xFileInfo = GetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.GetFileStream();
        }

        public static FileAttributes GetFileAttributes(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            return mVFS.GetFileAttributes(aPath);
        }

        public static void SetFileAttributes(string aPath, FileAttributes fileAttributes)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            mVFS.SetFileAttributes(aPath, fileAttributes);
        }

        public static bool IsValidDriveId(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            return mVFS.IsValidDriveId(aPath);
        }

        public static long GetTotalSize(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetTotalSize ---");
            return mVFS.GetTotalSize(aDriveId);
        }

        public static long GetAvailableFreeSpace(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetAvailableFreeSpace ---");
            return mVFS.GetAvailableFreeSpace(aDriveId);
        }

        public static long GetTotalFreeSpace(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetTotalFreeSpace ---");
            return mVFS.GetTotalFreeSpace(aDriveId);
        }

        public static string GetFileSystemType(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileSystemType ---");
            return mVFS.GetFileSystemType(aDriveId);
        }

        public static string GetFileSystemLabel(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileSystemLabel ---");
            return mVFS.GetFileSystemLabel(aDriveId);
        }

        public static void SetFileSystemLabel(string aDriveId, string aLabel)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.SetFileSystemLabel ---");
            mVFS.SetFileSystemLabel(aDriveId, aLabel);
        }

        public static void Format(string aDriveId, string aDriveFormat, bool aQuick)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.Format ---");
            mVFS.Format(aDriveId, aDriveFormat, aQuick);
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
            char[] xReturn =
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
            return xReturn;
        }

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
            char[] xReturn =
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
            return xReturn;
        }

        public static char GetPathSeparator()
        {
            return ';';
        }

        public static char[] GetRealInvalidPathChars()
        {
            char[] xReturn =
            {
                '"',
                '<',
                '>',
                '|'
            };
            return xReturn;
        }

        public static char[] GetTrimEndChars()
        {
            char[] xReturn =
            {
                (char) 0x9,
                (char) 0xA,
                (char) 0xB,
                (char) 0xC,
                (char) 0xD,
                (char) 0x20,
                (char) 0x85,
                (char) 0xA0
            };
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

        #endregion Helpers

        /// <summary>
        /// Gets the parent directory entry from the path.
        /// </summary>
        /// <param name="aPath">The full path to the current directory entry.</param>
        /// <returns>The parent directory entry.</returns>
        /// <exception cref="ArgumentException">Argument is null or empty</exception>
        /// <exception cref="NotImplementedException"></exception>
        public static DirectoryEntry GetParent(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetParent ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            if (aPath == Path.GetPathRoot(aPath))
            {
                return null;
            }

            string xFileOrDirectory = Path.HasExtension(aPath) ? Path.GetFileName(aPath) : Path.GetDirectoryName(aPath);
            if (xFileOrDirectory != null)
            {
                string xPath = aPath.Remove(aPath.Length - xFileOrDirectory.Length);
                return GetDirectory(xPath);
            }

            return null;
        }
    }
}
