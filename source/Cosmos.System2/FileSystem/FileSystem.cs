using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem
{
    /// <summary>
    /// FileSystem abstract class.
    /// </summary>
    public abstract class FileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">A partiton managed by the filesystem.</param>
        /// <param name="aRootPath">A root path.</param>
        /// <param name="aSize">A partition size.</param>
        protected FileSystem(Partition aDevice, string aRootPath, long aSize)
        {
            Device = aDevice;
            RootPath = aRootPath;
            Size = aSize;
        }

        /// <summary>
        /// Print filesystem info.
        /// </summary>
        /// <exception cref="IOException">Thrown on I/O error.</exception>
        public abstract void DisplayFileSystemInfo();

        /// <summary>
        /// Get list of sub-directories in a directory.
        /// </summary>
        /// <param name="baseDirectory">A base directory.</param>
        /// <returns>DirectoryEntry list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when baseDirectory is null / memory error.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory);

        /// <summary>
        /// Get root directory.
        /// </summary>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        public abstract DirectoryEntry GetRootDirectory();

        /// <summary>
        /// Create directory.
        /// </summary>
        /// <param name="aParentDirectory">A parent directory.</param>
        /// <param name="aNewDirectory">A new directory name.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aParentDirectory is null.</item>
        /// <item>aNewDirectory is null or empty.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
        public abstract DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory);

        /// <summary>
        /// Create file.
        /// </summary>
        /// <param name="aParentDirectory">A parent directory.</param>
        /// <param name="aNewFile">A new file name.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aParentDirectory is null.</item>
        /// <item>aNewFile is null or empty.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
        public abstract DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile);

        /// <summary>
        /// Delete directory.
        /// </summary>
        /// <param name="aDirectoryEntry">A directory entry to delete.</param>
        /// <exception cref="NotImplementedException">Thrown when given entry type is unknown.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when tring to delete root directory.</item>
        /// <item>directory entry type is invalid.</item>
        /// <item>data size invalid.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDirectoryEntry is null.</item>
        /// <item>aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>aData length is 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public abstract void DeleteDirectory(DirectoryEntry aPath);

        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="aDirectoryEntry">A directory entry to delete.</param>
        /// <exception cref="NotImplementedException">Thrown when given entry type is unknown.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when tring to delete root directory.</item>
        /// <item>directory entry type is invalid.</item>
        /// <item>data size invalid.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when data lenght is greater then Int32.MaxValue.</item>
        /// <item>The number of clusters in the FAT entry is greater than Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDirectoryEntry is null.</item>
        /// <item>aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>The size of the chain is less then zero.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when FAT type is unknown.</item>
        /// <item>aData length is 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public abstract void DeleteFile(DirectoryEntry aPath);

        /// <summary>
        /// Get device.
        /// </summary>
        protected Partition Device { get; }

        /// <summary>
        /// Get root path.
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        /// Get size.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Get available free space.
        /// </summary>
        public abstract long AvailableFreeSpace { get; }

        /// <summary>
        /// Get total free space.
        /// </summary>
        public abstract long TotalFreeSpace { get; }

        /// <summary>
        /// Get type.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Get label.
        /// </summary>
        public abstract string Label { get; set; }

        /// <summary>
        /// Format drive. (delete all)
        /// </summary>
        /// <param name="aDriveFormat">unused.</param>
        /// <param name="aQuick">unused.</param>
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
        /// <item>Data length is 0.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
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
        public abstract void Format(string aDriveFormat, bool aQuick);
    }
}
