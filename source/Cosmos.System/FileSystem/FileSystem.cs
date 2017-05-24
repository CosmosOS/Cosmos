using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem
{
    public abstract class FileSystem
    {
		public static FileSystemResolver Resolver { get; set; } = new FileSystemResolver();

        protected FileSystem(Partition aDevice, string aRootPath)
        {
            mDevice = aDevice;
            mRootPath = aRootPath;
        }

        public static FileSystemType GetFileSystemType(Partition aDevice)
        {
			return Resolver.Resolve(aDevice);
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

		protected Partition Device { get { return mDevice; } }

		public string RootPath { get { return mRootPath; } }
	}
}
