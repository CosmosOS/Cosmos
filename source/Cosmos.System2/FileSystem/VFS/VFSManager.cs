// #define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    /// <summary>
    /// VFSManager (Virtual File System Manager) class. Used to manage files and directories.
    /// </summary>
    public static class VFSManager
    {
        private static VFSBase mVFS;


        /// <summary>
        /// Register VFS. Initialize the VFS.
        /// </summary>
        /// <param name="aVFS">A VFS to register.</param>
        /// <exception cref="Exception">Thrown if VFS already registered / memory error.</exception>
        /// <exception cref="IOException">Thrown on I/O exception.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="PathTooLongException">Thrown on fatal error.</exception>
        /// <exception cref="System.Security.SecurityException">Thrown on fatal error.</exception>
        /// <exception cref="FileNotFoundException">Thrown on memory error.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown on fatal error.</exception>
        public static void RegisterVFS(VFSBase aVFS, bool aShowInfo = true, bool aAllowReinitialise = false)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.RegisterVFS ---");
            if (!aAllowReinitialise && mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            mVFS = aVFS;
            aVFS.Initialize(aShowInfo);
        }

        /// <summary>
        /// Create a file.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if aPath is null.</exception>
        /// <exception cref="ArgumentException">Thrown if aPath is empty or contains invalid chars.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when the entry at aPath is not a file.</item>
        /// <item>Thrown when the parent directory of aPath is not a directory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="PathTooLongException">Thrown when aPath is longer than the system defined max lenght.</exception>
        public static DirectoryEntry CreateFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.CreateFile ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }
            ThrowIfNotRegistered();
            Global.mFileSystemDebugger.SendInternal("aPath =" + aPath);

            return mVFS.CreateFile(aPath);
        }

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if VFS manager is null.</item>
        /// <item>The entry at aPath is not a file.</item>
        /// </list>
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the specified path isn't a file</exception>
        public static void DeleteFile(string aPath)
        {
            ThrowIfNotRegistered();
            var xFile = mVFS.GetFile(aPath);

            if (xFile.mEntryType != DirectoryEntryTypeEnum.File)
                throw new UnauthorizedAccessException("The specified path isn't a file");

            mVFS.DeleteFile(xFile);
        }

        /// <summary>
        /// Get file.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null / empty / invalid.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Filesystem is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static DirectoryEntry GetFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("-- VFSManager.GetFile --");
            ThrowIfNotRegistered();

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            string xFileName = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal("xFileName = " + xFileName);

            string xDirectory = aPath.Remove(aPath.Length - xFileName.Length);
            Global.mFileSystemDebugger.SendInternal("xDirectory = " + xDirectory);

            char xLastChar = xDirectory[xDirectory.Length - 1];
            if (xLastChar != Path.DirectorySeparatorChar)
            {
                xDirectory += Path.DirectorySeparatorChar;
            }

            var xList = GetDirectoryListing(xDirectory);
            for (int i = 0; i < xList.Count; i++)
            {
                var xEntry = xList[i];
                if (xEntry != null && xEntry.mEntryType == DirectoryEntryTypeEnum.File
                                   && xEntry.mName.ToUpper() == xFileName.ToUpper())
                {
                    return xEntry;
                }
            }

            return null;
        }

        /// <summary>
        /// Create directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null / empty / invalid.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>the entry at aPath is not a directory.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath length is zero.</item>
        /// <item>Thrown if aPath is invalid.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
        /// <exception cref="PathTooLongException">Thrown when The aPath is longer than the system defined maximum length.</exception>
        public static DirectoryEntry CreateDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("-- VFSManager.CreateDirectory -- " + aPath);
            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }
            ThrowIfNotRegistered();

            var directoryEntry = mVFS.CreateDirectory(aPath);
            Global.mFileSystemDebugger.SendInternal("-- -------------------------- -- " + aPath);
            return directoryEntry;
        }

        /// <summary>
        /// Delete directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <param name="recursive">Recursive delete (not empty directory).</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if VFSManager is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if VFSManager is null.</item>
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown if specified path isn't a directory / trying to delete not empty directory not recursivly / directory contains a corrupted file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when trying to delete unknown type entry.</exception>
        public static void DeleteDirectory(string aPath, bool recursive)
        {
            ThrowIfNotRegistered();
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

        /// <summary>
        /// Get directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if VFSManager is null.</item>
        /// <item>Thrown if aPath is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static DirectoryEntry GetDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectory ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }
            ThrowIfNotRegistered();

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            return mVFS.GetDirectory(aPath);
        }

        /// <summary>
        /// Get directory listing.
        /// </summary>
        /// <param name="aPath">A path to the entry.</param>
        /// <returns>DirectoryEntry list value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Filesystem is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectoryListing ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }
            ThrowIfNotRegistered();

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.GetDirectoryListing(aPath);
        }

        /// <summary>
        /// Get volume.
        /// </summary>
        /// <param name="aVolume">The volume root path.</param>
        /// <returns>A directory entry for the volume.</returns>
        /// <exception cref="ArgumentException">Thrown when aVolume is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aVolume</exception>
        /// <exception cref="ArgumentNullException">Thrown if aVolume / filesystem is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        public static DirectoryEntry GetVolume(string aVolume)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolume ---");

            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException(nameof(aVolume));
            }
            ThrowIfNotRegistered();

            Global.mFileSystemDebugger.SendInternal("aVolume =");
            Global.mFileSystemDebugger.SendInternal(aVolume);

            return mVFS.GetVolume(aVolume);
        }

        /// <summary>
        /// Gets the volumes for all registered file systems.
        /// </summary>
        /// <returns>A list of directory entries for all volumes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filesystem is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        public static List<DirectoryEntry> GetVolumes()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolumes ---");
            ThrowIfNotRegistered();

            return mVFS.GetVolumes();
        }

        /// <summary>
        /// Get logical drivers list.
        /// </summary>
        /// <returns>List of strings value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filesystem is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        public static List<string> GetLogicalDrives()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetLogicalDrives ---");

            List<string> xDrives = new List<string>();
            foreach (DirectoryEntry entry in GetVolumes())
                xDrives.Add(entry.mName + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives;
        }

        /// <summary>
        /// Check if file exists.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>bool value.</returns>
        public static bool FileExists(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("-- VFSManager.FileExists --");
            ThrowIfNotRegistered();

            if (aPath == null)
            {
                return false;
            }

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);
            string xPath = Path.GetFullPath(aPath);

            Global.mFileSystemDebugger.SendInternal("xPath = " + xPath);

            return GetFile(xPath) != null;
        }

        /// <summary>
        /// Check if file exists.
        /// </summary>
        /// <param name="aEntry">A entry of the file.</param>
        /// <returns>bool value.</returns>
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

        /// <summary>
        /// Check if directory exists.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>bool value.</returns>
        /// <exception cref="ArgumentException">Thrown when aPath is null or empty.</exception>
        public static bool DirectoryExists(string aPath)
        {
            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }
            ThrowIfNotRegistered();

            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");
            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            try
            {

                string xPath = Path.GetFullPath(aPath);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath = " + xPath);

                var result = GetDirectory(xPath) != null;
                Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists --- Returns: " + result);
                return result;
            }
            catch
            {
                /* Simply map any Exception to false as this method should return only bool */
                return false;
            }
        }

        /// <summary>
        /// Check if directory exists.
        /// </summary>
        /// <param name="aEntry">A entry of the directory.</param>
        /// <returns>bool value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when aEntry is null.</exception>
        public static bool DirectoryExists(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");

            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }
            ThrowIfNotRegistered();

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

        /// <summary>
        /// Get full path to the entry.
        /// </summary>
        /// <param name="aEntry">A entry.</param>
        /// <returns>string value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when aEntry is null.</exception>
        public static string GetFullPath(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFullPath ---");

            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }
            ThrowIfNotRegistered();

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

        /// <summary>
        /// Get file stream.
        /// </summary>
        /// <param name="aPathname">A path to the file.</param>
        /// <returns>Stream value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPathname is null / empty / invalid.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPathname is null or empty.</item>
        /// <item>Filesystem is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPathname is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// <item>The number of clusters in the FAT entry is greater than Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aPathname.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// <item>FAT table not found.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static Stream GetFileStream(string aPathname)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileStream ---");

            if (string.IsNullOrEmpty(aPathname))
            {
                return null;
            }
            ThrowIfNotRegistered();

            Global.mFileSystemDebugger.SendInternal("aPathName =");
            Global.mFileSystemDebugger.SendInternal(aPathname);

            var xFileInfo = GetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.GetFileStream();
        }

        /// <summary>
        /// Get file attributes.
        /// </summary>
        /// <param name="aPath">A path to the file</param>
        /// <returns>FileAttributes value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null or empty.</item>
        /// <item>Thrown when aFS root path is null or empty.</item>
        /// <item>Thrown on memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if VFSManager is null.</item>
        /// <item>Thrown when root directory is null.</item>
        /// <item>Thrown on memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Thrown on memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Thrown when data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when data size invalid.</item>
        /// <item>Thrown on invalid directory entry type.</item>
        /// <item>Thrown when aPath entry not found.</item>
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>Thrown aPath is neither a file neither a directory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static FileAttributes GetFileAttributes(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            ThrowIfNotRegistered();
            return mVFS.GetFileAttributes(aPath);
        }

        /// <summary>
        /// Sets the attributes for a File / Directory.
        /// Not implemented.
        /// </summary>
        /// <param name="aPath">The path of the File / Directory.</param>
        /// <param name="fileAttributes">The attributes of the File / Directory.</param>
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public static void SetFileAttributes(string aPath, FileAttributes fileAttributes)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            ThrowIfNotRegistered();
            mVFS.SetFileAttributes(aPath, fileAttributes);
        }

        /// <summary>
        /// Check if drive id is valid.
        /// </summary>
        /// <param name="driveId">Drive id to check.</param>
        /// <returns>bool value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if aPath length is smaller then 2, or greater than Int32.MaxValue.</exception>
        public static bool IsValidDriveId(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileAttributes ---");
            ThrowIfNotRegistered();
            return mVFS.IsValidDriveId(aPath);
        }

        /// <summary>
        /// Get total size in bytes.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static long GetTotalSize(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetTotalSize ---");
            ThrowIfNotRegistered();
            return mVFS.GetTotalSize(aDriveId);
        }

        /// <summary>
        /// Get available free space.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static long GetAvailableFreeSpace(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetAvailableFreeSpace ---");
            ThrowIfNotRegistered();
            return mVFS.GetAvailableFreeSpace(aDriveId);
        }

        /// <summary>
        /// Get total free space.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static long GetTotalFreeSpace(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetTotalFreeSpace ---");
            ThrowIfNotRegistered();
            return mVFS.GetTotalFreeSpace(aDriveId);
        }

        /// <summary>
        /// Get file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static string GetFileSystemType(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileSystemType ---");
            ThrowIfNotRegistered();
            return mVFS.GetFileSystemType(aDriveId);
        }

        /// <summary>
        /// Get file system label.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static string GetFileSystemLabel(string aDriveId)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileSystemLabel ---");
            ThrowIfNotRegistered();
            return mVFS.GetFileSystemLabel(aDriveId);
        }

        /// <summary>
        /// Set file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <param name="aLabel">A label to be set.</param>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public static void SetFileSystemLabel(string aDriveId, string aLabel)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.SetFileSystemLabel ---");
            ThrowIfNotRegistered();
            mVFS.SetFileSystemLabel(aDriveId, aLabel);
        }

        #region Helpers

        /// <summary>
        /// Get alt. directory separator char.
        /// </summary>
        /// <returns>char value.</returns>
        public static char GetAltDirectorySeparatorChar()
        {
            return '/';
        }

        /// <summary>
        /// Get directory separator char.
        /// </summary>
        /// <returns>char value.</returns>
        public static char GetDirectorySeparatorChar()
        {
            return '\\';
        }

        /// <summary>
        /// Get invalid filename chars.
        /// </summary>
        /// <returns>char array value.</returns>
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

        /// <summary>
        /// Get invalid path chars with additional checks.
        /// </summary>
        /// <returns>char array value.</returns>
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

        /// <summary>
        /// Get path separator char.
        /// </summary>
        /// <returns>char value.</returns>
        public static char GetPathSeparator()
        {
            return ';';
        }

        /// <summary>
        /// Get real invalid path chars.
        /// </summary>
        /// <returns>char array value.</returns>
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

        /// <summary>
        /// Get trim end chars.
        /// </summary>
        /// <returns>char array value.</returns>
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

        /// <summary>
        /// Get volume separator char.
        /// </summary>
        /// <returns>char value.</returns>
        public static char GetVolumeSeparatorChar()
        {
            return ':';
        }

        /// <summary>
        /// Get max path.
        /// </summary>
        /// <returns>int value.</returns>
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

        /// <summary>
        /// Split path.
        /// </summary>
        /// <param name="aPath">A path to split.</param>
        /// <returns>string array.</returns>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        public static string[] SplitPath(string aPath)
        {
            //TODO: This should call Path.GetDirectoryName() and then loop calling Directory.GetParent(), but those aren't implemented yet.
            return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Get directory separators.
        /// </summary>
        /// <returns>char array value.</returns>
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
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null / empty / invalid.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if VFSManager is null.</item>
        /// <item>Thrown if aPath is null.</item>
        /// <item>Root directory is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when root directory address is smaller then root directory address.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when aPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public static DirectoryEntry GetParent(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetParent ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }
            ThrowIfNotRegistered();

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


        /// <summary>
        /// Cleans up manager if the VFS has to be reintialised.
        /// </summary>
        internal static void Reset()
        {
            mVFS = null;
        }
        /// <summary>
        /// Gets the next file system letter. For internal cosmos use only.
        /// </summary>
        /// <returns>Next file system letter</returns>
        public static string GetNextFilesystemLetter()
        {
            ThrowIfNotRegistered();
            return mVFS.GetNextFilesystemLetter();
        }
        /// <summary>
        /// Gets all of the disks
        /// </summary>
        /// <returns>All of the disks on the system</returns>
        public static List<Disk> GetDisks()
        {
            ThrowIfNotRegistered();
            return mVFS.GetDisks();
        }
        /// <summary>
        /// Throws an Exception if VFS is not registered.
        /// </summary>
        public static void ThrowIfNotRegistered()
        {
            if (mVFS == null)
            {
                throw new Exception("VFS not registered. Use VFSManager.RegisterVFS() before doing file system operations!");
            }
        }
    }
}
