//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Cosmos default virtual file system.
    /// </summary>
    /// <seealso cref="Cosmos.System.FileSystem.VFS.VFSBase" />
    public class CosmosVFS : VFSBase
    {
        private List<Partition> mPartitions;
        private List<FileSystem> mFileSystems;
        private FileSystem mCurrentFileSystem;
        private List<FileSystemFactory> mRegisteredFileSystems;

        /// <summary>
        /// Initializes the virtual file system.
        /// </summary>
        /// <exception cref="IOException">Thrown on I/O exception.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="Exception">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="PathTooLongException">Thrown on fatal error.</exception>
        /// <exception cref="System.Security.SecurityException">Thrown on fatal error.</exception>
        /// <exception cref="FileNotFoundException">Thrown on memory error.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown on fatal error.</exception>
        public override void Initialize()
        {
            mPartitions = new List<Partition>();
            mFileSystems = new List<FileSystem>();
            mRegisteredFileSystems = new List<FileSystemFactory>();

            RegisterFileSystem(new FatFileSystemFactory());

            InitializePartitions();
            if (mPartitions.Count > 0)
            {
                InitializeFileSystems();
            }
        }

        /// <summary>
        /// Register file system.
        /// </summary>
        /// <param name="aFileSystemFactory">A file system to register.</param>
        public override void RegisterFileSystem(FileSystemFactory aFileSystemFactory)
        {
            Global.mFileSystemDebugger.SendInternal($"Registering filesystem {aFileSystemFactory.Name}");
            mRegisteredFileSystems.Add(aFileSystemFactory);
        }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="aPath">The full path including the file to create.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null.</item>
        /// <item>aNewDirectory is null or empty.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>the entry at aPath is not a file.</item>
        /// <item>Thrown when the parent directory of aPath is not a directory.</item>
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
        public override DirectoryEntry CreateFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- CosmosVFS.CreateFile ---");

            if (aPath == null)
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("aPath is empty");
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            if (File.Exists(aPath))
            {
                Global.mFileSystemDebugger.SendInternal("File already exists.");
                return GetFile(aPath);
            }
            Global.mFileSystemDebugger.SendInternal("File doesn't exist.");

            string xFileToCreate = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal("After GetFileName");
            Global.mFileSystemDebugger.SendInternal("xFileToCreate =");
            Global.mFileSystemDebugger.SendInternal(xFileToCreate);

            string xParentDirectory = Path.GetDirectoryName(aPath);
            Global.mFileSystemDebugger.SendInternal("After removing last path part");
            Global.mFileSystemDebugger.SendInternal("xParentDirectory =");
            Global.mFileSystemDebugger.SendInternal(xParentDirectory);

            DirectoryEntry xParentEntry = GetDirectory(xParentDirectory);
            if (xParentEntry == null)
            {
                Global.mFileSystemDebugger.SendInternal("Parent directory doesn't exist.");
                xParentEntry = CreateDirectory(xParentDirectory);
            }
            Global.mFileSystemDebugger.SendInternal("Parent directory exists.");

            var xFS = GetFileSystemFromPath(xParentDirectory);
            return xFS.CreateFile(xParentEntry, xFileToCreate);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="aPath">The full path including the directory to create.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aPath is null.</item>
        /// <item>aNewDirectory is null or empty.</item>
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
        public override DirectoryEntry CreateDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("-- CosmosVFS.CreateDirectory ---");

            if (aPath == null)
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("aPath length is zero");
            }

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            if (Directory.Exists(aPath))
            {
                Global.mFileSystemDebugger.SendInternal("Path already exists.");
                return GetDirectory(aPath);
            }

            Global.mFileSystemDebugger.SendInternal("Path doesn't exist.");

            aPath = aPath.TrimEnd(DirectorySeparatorChar, AltDirectorySeparatorChar);

            string xDirectoryToCreate = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal("After GetFileName");
            Global.mFileSystemDebugger.SendInternal("xDirectoryToCreate = " + xDirectoryToCreate);

            string xParentDirectory = Path.GetDirectoryName(aPath);
            Global.mFileSystemDebugger.SendInternal("After removing last path part");
            Global.mFileSystemDebugger.SendInternal("xParentDirectory = " + xParentDirectory);

            DirectoryEntry xParentEntry = GetDirectory(xParentDirectory);

            if (xParentEntry == null)
            {
                Global.mFileSystemDebugger.SendInternal("Parent directory doesn't exist.");
                xParentEntry = CreateDirectory(xParentDirectory);
            }

            Global.mFileSystemDebugger.SendInternal("Parent directory exists.");

            var xFS = GetFileSystemFromPath(xParentDirectory);
            return xFS.CreateDirectory(xParentEntry, xDirectoryToCreate);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="aPath">The full path.</param>
        /// <returns>TRUE on success.</returns>
        public override bool DeleteFile(DirectoryEntry aPath)
        {
            try
            {
                var xFS = GetFileSystemFromPath(aPath.mFullPath);
                xFS.DeleteFile(aPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes an empty directory.
        /// </summary>
        /// <param name="aPath">The full path.</param>
        /// <returns>TRUE on success.</returns>
        public override bool DeleteDirectory(DirectoryEntry aPath)
        {
            try
            {
                if (GetDirectoryListing(aPath).Count > 0)
                {
                    throw new Exception("Directory is not empty");
                }

                var xFS = GetFileSystemFromPath(aPath.mFullPath);
                xFS.DeleteDirectory(aPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the directory listing for a path.
        /// </summary>
        /// <param name="aPath">The full path.</param>
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
        /// <item>Thrown if aFS is null.</item>
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
        public override List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("-- CosmosVFS.GetDirectoryListing --");
            var xFS = GetFileSystemFromPath(aPath);
            var xDirectory = DoGetDirectoryEntry(aPath, xFS);
            return xFS.GetDirectoryListing(xDirectory);
        }

        /// <summary>
        /// Gets the directory listing for a directory entry.
        /// </summary>
        /// <param name="aDirectory">The directory entry.</param>
        /// <returns>DirectoryEntry type list value.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if aDirectory is null or empty.</item>
        /// <item>aDirectory.mFullPath is null or empty.</item>
        /// <item>Memory error.</item>
        /// <item>Fatal error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if aFS is null.</item>
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
        /// <item>Thrown when aDirectory.mFullPath is too deep.</item>
        /// <item>Data lenght is greater then Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when unable to determine filesystem for path:  + aDirectory.mFullPath.</item>
        /// <item>data size invalid.</item>
        /// <item>invalid directory entry type.</item>
        /// <item>path not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aDirectory)
        {
            if (aDirectory == null || String.IsNullOrEmpty(aDirectory.mFullPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aDirectory));
            }

            return GetDirectoryListing(aDirectory.mFullPath);
        }

        /// <summary>
        /// Gets the directory entry for a directory.
        /// </summary>
        /// <param name="aPath">The full path path.</param>
        /// <returns>A directory entry for the directory.</returns>
        /// <exception cref="Exception">Thrown when the entry at aPath is not a directory.</exception>
        public override DirectoryEntry GetDirectory(string aPath)
        {
            try
            {
                var xFileSystem = GetFileSystemFromPath(aPath);
                var xEntry = DoGetDirectoryEntry(aPath, xFileSystem);
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.Directory))
                {
                    return xEntry;
                }
            }
            catch (Exception)
            {
                Global.mFileSystemDebugger.SendInternal("CosmosVFS.GetDirectory - DoGetDirectoryEntry failed, returning null. aPath = " + aPath);
                return null;
            }
            throw new Exception(aPath + " was found, but is not a directory.");
        }

        /// <summary>
        /// Gets the directory entry for a file.
        /// </summary>
        /// <param name="aPath">The full path.</param>
        /// <returns>A directory entry for the file.</returns>
        /// <exception cref="Exception">Thrown when the entry at aPath is not a file.</exception>
        public override DirectoryEntry GetFile(string aPath)
        {
            try
            {
                var xFileSystem = GetFileSystemFromPath(aPath);
                var xEntry = DoGetDirectoryEntry(aPath, xFileSystem);
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.File))
                {
                    return xEntry;
                }
            }
            catch (Exception)
            {
                Global.mFileSystemDebugger.SendInternal("CosmosVFS.GetFile - DoGetDirectoryEntry failed, returning null. aPath = " + aPath);
                return null;
            }
            throw new Exception(aPath + " was found, but is not a file.");
        }

        /// <summary>
        /// Gets the volumes for all registered file systems.
        /// </summary>
        /// <returns>A list of directory entries for all volumes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if filesystem is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        public override List<DirectoryEntry> GetVolumes()
        {
            List<DirectoryEntry> xVolumes = new List<DirectoryEntry>();

            for (int i = 0; i < mFileSystems.Count; i++)
            {
                xVolumes.Add(GetVolume(mFileSystems[i]));
            }

            return xVolumes;
        }

        /// <summary>
        /// Gets the directory entry for a volume.
        /// </summary>
        /// <param name="aPath">The volume root path.</param>
        /// <returns>A directory entry for the volume.</returns>
        /// <exception cref="ArgumentException">Thrown when aPath is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aPath</exception>
        /// <exception cref="ArgumentNullException">Thrown if filesystem is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        public override DirectoryEntry GetVolume(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                return null;
            }

            var xFileSystem = GetFileSystemFromPath(aPath);
            if (xFileSystem != null)
            {
                return GetVolume(xFileSystem);
            }

            return null;
        }

        /// <summary>
        /// Gets the attributes for a File / Directory.
        /// </summary>
        /// <param name="aPath">The path of the File / Directory.</param>
        /// <returns>The File / Directory attributes.</returns>
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
        /// <item>Thrown if aFS is null.</item>
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
        public override FileAttributes GetFileAttributes(string aPath)
        {
            /*
             * We are limiting ourselves to the simpler attributes File and Directory for now.
             * I think that in the end FAT does not support anything else
             */
            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.GetFileAttributes() for path {aPath}");

            var xFileSystem = GetFileSystemFromPath(aPath);
            var xEntry = DoGetDirectoryEntry(aPath, xFileSystem);

            if (xEntry == null)
                throw new Exception($"{aPath} is neither a file neither a directory");

            switch (xEntry.mEntryType)
            {
                case DirectoryEntryTypeEnum.File:
                    Global.mFileSystemDebugger.SendInternal($"It is a File");
                    return FileAttributes.Normal;

                case DirectoryEntryTypeEnum.Directory:
                    Global.mFileSystemDebugger.SendInternal($"It is a Directory");
                    return FileAttributes.Directory;

                case DirectoryEntryTypeEnum.Unknown:
                default:
                    throw new Exception($"{aPath} is neither a file neither a directory");
            }
        }

        /// <summary>
        /// Sets the attributes for a File / Directory.
        /// Not implemented.
        /// </summary>
        /// <param name="aPath">The path of the File / Directory.</param>
        /// <param name="fileAttributes">The attributes of the File / Directory.</param>
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public override void SetFileAttributes(string aPath, FileAttributes fileAttributes)
        {
            throw new NotImplementedException("SetFileAttributes not implemented");
        }

        /// <summary>
        /// Initializes the partitions for all block devices.
        /// </summary>
        /// <exception cref="IOException">Thrown on I/O exception.</exception>
        protected virtual void InitializePartitions()
        {
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                if (BlockDevice.Devices[i] is Partition)
                {
                    mPartitions.Add((Partition)BlockDevice.Devices[i]);
                }
            }

            if (mPartitions.Count > 0)
            {
                for (int i = 0; i < mPartitions.Count; i++)
                {
                    Global.mFileSystemDebugger.SendInternal("Partition #: ");
                    Global.mFileSystemDebugger.SendInternal(i + 1);
                    global::System.Console.WriteLine("Partition #: " + (i + 1));
                    Global.mFileSystemDebugger.SendInternal("Block Size:");
                    Global.mFileSystemDebugger.SendInternal(mPartitions[i].BlockSize);
                    global::System.Console.WriteLine("Block Size: " + mPartitions[i].BlockSize + " bytes");
                    Global.mFileSystemDebugger.SendInternal("Block Count:");
                    Global.mFileSystemDebugger.SendInternal(mPartitions[i].BlockCount);
                    global::System.Console.WriteLine("Block Count: " + mPartitions[i].BlockCount);
                    Global.mFileSystemDebugger.SendInternal("Size:");
                    Global.mFileSystemDebugger.SendInternal(mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024);
                    global::System.Console.WriteLine("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                global::System.Console.WriteLine("No partitions found!");
            }
        }

        /// <summary>
        /// Initializes the file system for all partitions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if null partition exists.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown on I/O exception.</exception>
        /// <exception cref="PathTooLongException">Thrown on fatal error.</exception>
        /// <exception cref="System.Security.SecurityException">Thrown on fatal error.</exception>
        /// <exception cref="FileNotFoundException">Thrown on memory error.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown on fatal error.</exception>
        protected virtual void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, VolumeSeparatorChar, DirectorySeparatorChar);
                var xSize = (long)(mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024);

                // We 'probe' the partition <i> with all the FileSystem registered until we find a Filesystem that can read / write to it
                foreach (var fs in mRegisteredFileSystems)
                {
                    if (fs.IsType(mPartitions[i]))
                    {
                        Global.mFileSystemDebugger.SendInternal($"Partion {i} has a {fs.Name} filesystem");
                        mFileSystems.Add(fs.Create(mPartitions[i], xRootPath, xSize));
                    }
                }

                if ((mFileSystems.Count > 0) && (mFileSystems[mFileSystems.Count - 1].RootPath == xRootPath))
                {
                    string xMessage = string.Concat("Initialized ", mFileSystems.Count, " filesystem(s)...");
                    global::System.Console.WriteLine(xMessage);
                    mFileSystems[i].DisplayFileSystemInfo();
                }
                else
                {
                    string xMessage = string.Concat("No filesystem found on partition #", i);
                    global::System.Console.WriteLine(xMessage);
                }
            }
        }

        /// <summary>
        /// Gets the file system from a path.
        /// </summary>
        /// <param name="aPath">The path.</param>
        /// <returns>The file system for the path.</returns>
        /// <exception cref="ArgumentException">Thrown when aPath is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aPath</exception>
        private FileSystem GetFileSystemFromPath(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- CosmosVFS.GetFileSystemFromPath ---");

            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            string xPath = Path.GetPathRoot(aPath);
            Global.mFileSystemDebugger.SendInternal("xPath after GetPathRoot = " + xPath);

            if ((mCurrentFileSystem != null) && (xPath == mCurrentFileSystem.RootPath))
            {
                Global.mFileSystemDebugger.SendInternal("Returning current file system.");
                return mCurrentFileSystem;
            }

            for (int i = 0; i < mFileSystems.Count; i++)
            {
                if (mFileSystems[i].RootPath == xPath)
                {
                    Global.mFileSystemDebugger.SendInternal("Found filesystem.");
                    mCurrentFileSystem = mFileSystems[i];
                    return mCurrentFileSystem;
                }
            }
            throw new Exception("Unable to determine filesystem for path: " + aPath);
        }

        /// <summary>
        /// Attempts to get a directory entry for a path in a file system.
        /// </summary>
        /// <param name="aPath">The path.</param>
        /// <param name="aFS">The file system.</param>
        /// <returns>A directory entry for the path.</returns>
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
        /// <item>Thrown if aFS is null.</item>
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
        /// </list>
        /// </exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        private DirectoryEntry DoGetDirectoryEntry(string aPath, FileSystem aFS)
        {
            Global.mFileSystemDebugger.SendInternal("--- CosmosVFS.DoGetDirectoryEntry ---");

            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }

            if (aFS == null)
            {
                throw new ArgumentNullException(nameof(aFS));
            }

            Global.mFileSystemDebugger.SendInternal("aPath = " + aPath);

            string[] xPathParts = VFSManager.SplitPath(aPath);

            DirectoryEntry xBaseDirectory = GetVolume(aFS);

            if (xPathParts.Length == 1)
            {
                Global.mFileSystemDebugger.SendInternal("Returning the volume.");
                return xBaseDirectory;
            }

            // start at index 1, because 0 is the volume
            for (int i = 1; i < xPathParts.Length; i++)
            {
                var xPathPart = xPathParts[i].ToLower();

                var xPartFound = false;
                var xListing = aFS.GetDirectoryListing(xBaseDirectory);

                for (int j = 0; j < xListing.Count; j++)
                {
                    var xListingItem = xListing[j];
                    string xListingItemName = xListingItem.mName.ToLower();
                    xPathPart = xPathPart.ToLower();

                    if (xListingItemName == xPathPart)
                    {
                        xBaseDirectory = xListingItem;
                        Global.mFileSystemDebugger.SendInternal("Now checking: " + xBaseDirectory.mFullPath);
                        xPartFound = true;
                        break;
                    }
                }

                if (!xPartFound)
                {
                    throw new Exception("Path part '" + xPathPart + "' not found!");
                }
            }

            Global.mFileSystemDebugger.SendInternal("Returning: " + xBaseDirectory.mFullPath);
            return xBaseDirectory;
        }

        /// <summary>
        /// Gets the root directory entry for a volume in a file system.
        /// </summary>
        /// <param name="aFS">The file system containing the volume.</param>
        /// <returns>A directory entry for the volume.</returns>
        /// <exception cref="ArgumentNullException">Thrown if aFS is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        private DirectoryEntry GetVolume(FileSystem aFS)
        {
            Global.mFileSystemDebugger.SendInternal("--- CosmosVFS.GetVolume ---");

            if (aFS == null)
            {
                Global.mFileSystemDebugger.SendInternal("File system is null.");
                throw new ArgumentNullException(nameof(aFS));
            }

            return aFS.GetRootDirectory();
        }

        /// <summary>
        /// Verifies if driveId is a valid id for a drive.
        /// </summary>
        /// <param name="driveId">The id of the drive.</param>
        /// <returns>true if the drive id is valid, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if driveId length is smaller then 2, or greater than Int32.MaxValue.</exception>
        public override bool IsValidDriveId(string driveId)
        {
            Global.mFileSystemDebugger.SendInternal($"driveId is {driveId} after normalization");

            /* We need to remove ':\' to get only the numeric value */
            driveId = driveId.Remove(driveId.Length - 2);
            Global.mFileSystemDebugger.SendInternal($"driveId is now {driveId}");

            /*
             * Cosmos Drive name is really similar to DOS / Windows but a number instead of a letter is used, it is not limited
             * to 1 character but any number is valid
             */

            bool isOK = Int32.TryParse(driveId, out int val);
            Global.mFileSystemDebugger.SendInternal($"isOK is {isOK}");

            return isOK;
        }

        /// <summary>
        /// Get total size in bytes.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override long GetTotalSize(string aDriveId)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            /* We have to return it in bytes */
            return xFs.Size * 1024 * 1024;
        }

        /// <summary>
        /// Get available free space.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override long GetAvailableFreeSpace(string aDriveId)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            return xFs.AvailableFreeSpace;
        }

        /// <summary>
        /// Get total free space.
        /// </summary>
        /// <param name="aDriveId">A drive id to get the size of.</param>
        /// <returns>long value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override long GetTotalFreeSpace(string aDriveId)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            return xFs.TotalFreeSpace;
        }

        /// <summary>
        /// Get file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override string GetFileSystemType(string aDriveId)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            return xFs.Type;
        }

        /// <summary>
        /// Get file system label.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override string GetFileSystemLabel(string aDriveId)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            return xFs.Label;
        }

        /// <summary>
        /// Set file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <param name="aLabel">A label to be set.</param>
        /// <exception cref="ArgumentException">Thrown when aDriveId is null or empty.</exception>
        /// <exception cref="Exception">Unable to determine filesystem for path:  + aDriveId</exception>
        public override void SetFileSystemLabel(string aDriveId, string aLabel)
        {
            Global.mFileSystemDebugger.SendInternal("--- CosmosVFS.SetFileSystemLabel ---");
            Global.mFileSystemDebugger.SendInternal($"aDriveId {aDriveId} aLabel {aLabel}");

            var xFs = GetFileSystemFromPath(aDriveId);
            xFs.Label = aLabel;
        }

        /// <summary>
        /// Format partition.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <param name="aDriveFormat">A drive format.</param>
        /// <param name="aQuick">Quick format.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null / memory error.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when aDriveId is null or empty.</item>
        /// <item>Data length is 0.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Unable to determine filesystem for path:  + aDriveId.</item>
        /// <item>Thrown when data size invalid.</item>
        /// <item>Thrown on unknown file system type.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="NotImplementedException">Thrown when FAT type is unknown.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public override void Format(string aDriveId, string aDriveFormat, bool aQuick)
        {
            var xFs = GetFileSystemFromPath(aDriveId);

            xFs.Format(aDriveFormat, aQuick);
        }

    }
}
