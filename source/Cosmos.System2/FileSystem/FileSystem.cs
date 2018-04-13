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
        protected FileSystem(Partition aDevice, string aRootPath, long aSize)
        {
            mDevice = aDevice;
            mRootPath = aRootPath;
            mSize = aSize;
        }

        public abstract void DisplayFileSystemInfo();

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory);

        public abstract DirectoryEntry GetRootDirectory();

        public abstract DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory);

        public abstract DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile);

        public abstract void DeleteDirectory(DirectoryEntry aPath);

        public abstract void DeleteFile(DirectoryEntry aPath);

        protected Partition mDevice { get; }

        public string mRootPath { get; }

        public long mSize { get; }

        public abstract long mAvailableFreeSpace { get; }

        public abstract long mTotalFreeSpace { get; }

        public abstract string mType { get; }

        public abstract string mLabel { get; set; } 
    }
}
