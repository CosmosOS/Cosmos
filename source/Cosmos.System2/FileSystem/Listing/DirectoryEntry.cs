using System;
using System.IO;

namespace Cosmos.System.FileSystem.Listing
{
    /// <summary>
    /// Enumeration for the directory entry type.
    /// </summary>
    public enum DirectoryEntryTypeEnum
    {
        /// <summary>
        /// Directory
        /// </summary>
        Directory,

        /// <summary>
        /// File
        /// </summary>
        File,

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// A generic file system directory entry.
    /// </summary>
    public abstract class DirectoryEntry
    {
        public long mSize;
        public string mFullPath;
        public string mName;
        protected readonly FileSystem mFileSystem;
        public readonly DirectoryEntry mParent;
        public readonly DirectoryEntryTypeEnum mEntryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryEntry"/> class.
        /// </summary>
        /// <param name="aFileSystem">The file system that contains the directory entry.</param>
        /// <param name="aParent">The parent directory entry or null if the current entry is the root.</param>
        /// <param name="aFullPath">The full path to the entry.</param>
        /// <param name="aName">The entry name.</param>
        /// <param name="aSize">The size of the entry.</param>
        /// <param name="aEntryType">The ype of the entry.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">
        /// Argument is null or empty
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        protected DirectoryEntry(FileSystem aFileSystem, DirectoryEntry aParent, string aFullPath, string aName, long aSize, DirectoryEntryTypeEnum aEntryType)
        {
            if (aFileSystem == null)
            {
                throw new ArgumentNullException(nameof(aFileSystem));
            }
            if (string.IsNullOrEmpty(aFullPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aFullPath));
            }
            if (string.IsNullOrEmpty(aName))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aName));
            }

            mFileSystem = aFileSystem;
            mParent = aParent;
            mEntryType = aEntryType;
            mName = aName;
            mSize = aSize;
            mFullPath = aFullPath;
        }

        public abstract void SetName(string aName);

        public abstract void SetSize(long aSize);

        public abstract Stream GetFileStream();

        public abstract long GetUsedSpace();
    }
}
