using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem
{
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

        public abstract void DisplayFileSystemInfo();

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory);

        public abstract DirectoryEntry GetRootDirectory();

        public abstract DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory);

        public abstract DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile);

        public abstract void DeleteDirectory(DirectoryEntry aPath);

        public abstract void DeleteFile(DirectoryEntry aPath);

        protected Partition Device { get; }

        public string RootPath { get; }

        public long Size { get; }

        public abstract long AvailableFreeSpace { get; }

        public abstract long TotalFreeSpace { get; }

        public abstract string Type { get; }

        public abstract string Label { get; set; }

        public abstract void Format(string aDriveFormat, bool aQuick);
    }
}
