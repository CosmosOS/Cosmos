using System.Collections.Generic;
using Cosmos.System.FileSystem.Listing;
using System.IO;

namespace Cosmos.System.FileSystem.VFS
{
    /// <summary>
    /// Virtual file system base abstract class. 
    /// </summary>
    public abstract class VFSBase
    {
        /// <summary>
        /// Initializes the <see cref="VFSBase"/> system.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Register file system.
        /// </summary>
        /// <param name="aFileSystemFactory">A file system to register.</param>
        public abstract void RegisterFileSystem(FileSystemFactory aFileSystemFactory);

        /// <summary>
        /// Create File.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>DirectoryEntry value.</returns>
        public abstract DirectoryEntry CreateFile(string aPath);

        /// <summary>
        /// Create directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>DirectoryEntry value.</returns>
        public abstract DirectoryEntry CreateDirectory(string aPath);

        /// <summary>
        /// Delete File.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>bool value.</returns>
        public abstract bool DeleteFile(DirectoryEntry aPath);

        /// <summary>
        /// Delete directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>bool value.</returns>
        public abstract bool DeleteDirectory(DirectoryEntry aPath);

        /// <summary>
        /// Get directory.
        /// </summary>
        /// <param name="aPath">A path to the directory.</param>
        /// <returns>DirectoryEntry value.</returns>
        public abstract DirectoryEntry GetDirectory(string aPath);

        /// <summary>
        /// Get file.
        /// </summary>
        /// <param name="aPath">A path to the file.</param>
        /// <returns>DirectoryEntry value.</returns>
        public abstract DirectoryEntry GetFile(string aPath);

        /// <summary>
        /// Get directory listing.
        /// </summary>
        /// <param name="aPath">A path to the entry.</param>
        /// <returns>DirectoryEntry list value.</returns>
        public abstract List<DirectoryEntry> GetDirectoryListing(string aPath);

        /// <summary>
        /// Get directory listing.
        /// </summary>
        /// <param name="aEntry">A entry.</param>
        /// <returns>DirectoryEntry list value.</returns>
        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aEntry);

        /// <summary>
        /// Get volume.
        /// </summary>
        /// <param name="aVolume">A volume root path.</param>
        /// <returns>DirectoryEntry value.</returns>
        public abstract DirectoryEntry GetVolume(string aVolume);

        /// <summary>
        /// Get list of directory entrys for all volumes.
        /// </summary>
        /// <returns>DirectoryEntry list value.</returns>
        public abstract List<DirectoryEntry> GetVolumes();

        /// <summary>
        /// Gets the attributes for a File / Directory.
        /// </summary>
        /// <param name="aPath">The path of the File / Directory.</param>
        /// <returns>FileAttributes value.</returns>
        public abstract FileAttributes GetFileAttributes(string aPath);

        /// <summary>
        /// Sets the attributes for a File / Directory.
        /// </summary>
        /// <param name="aPath">The path of the File / Directory.</param>
        /// <param name="fileAttributes">The attributes of the File / Directory.</param>
        public abstract void SetFileAttributes(string aPath, FileAttributes fileAttributes);

        /// <summary>
        /// Get the directory separator char.
        /// </summary>
        public static char DirectorySeparatorChar { get { return '\\'; } }

        /// <summary>
        /// Get the alt. directory separator char.
        /// </summary>
        public static char AltDirectorySeparatorChar { get { return '/'; } }

        /// <summary>
        /// Get the volume separator char.
        /// </summary>
        public static char VolumeSeparatorChar { get { return ':'; } }

        /// <summary>
        /// Check if drive id is valid.
        /// </summary>
        /// <param name="driveId">Drive id to check.</param>
        /// <returns>bool value.</returns>
        public abstract bool IsValidDriveId(string driveId);

        /// <summary>
        /// Get the total size of the partition.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>long value.</returns>
        public abstract long GetTotalSize(string aDriveId);

        /// <summary>
        /// Get avilable free space in the partition.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>long value.</returns>
        public abstract long GetAvailableFreeSpace(string aDriveId);

        /// <summary>
        /// Get total free space in the partition.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>long value.</returns>
        public abstract long GetTotalFreeSpace(string aDriveId);

        /// <summary>
        /// Get file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        public abstract string GetFileSystemType(string aDriveId);

        /// <summary>
        /// Get file system label.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <returns>string value.</returns>
        public abstract string GetFileSystemLabel(string aDriveId);

        /// <summary>
        /// Set file system type.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <param name="aLabel">A label to be set.</param>
        public abstract void SetFileSystemLabel(string aDriveId, string aLabel);

        /// <summary>
        /// Format partition.
        /// </summary>
        /// <param name="aDriveId">A drive id.</param>
        /// <param name="aDriveFormat">A drive format.</param>
        /// <param name="aQuick">Quick format.</param>
        public abstract void Format(string aDriveId, string aDriveFormat, bool aQuick);
    }
}
