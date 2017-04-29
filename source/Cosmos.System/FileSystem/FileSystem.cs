using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem
{
    public abstract class FileSystem
    {
		public static Dictionary<FileSystemType, Func<Partition, bool>> FileSystems = new Dictionary<FileSystemType, Func<Partition, bool>>()
		{
			[FileSystemType.FAT] = FatFileSystem.IsDeviceFat
		};

        protected FileSystem(Partition aDevice, string aRootPath)
        {
            mDevice = aDevice;
            mRootPath = aRootPath;
        }

        public static FileSystemType GetFileSystemType(Partition aDevice)
        {
			foreach (var item in FileSystems)
			{
				if (item.Value(aDevice)) return item.Key;
			}

            return FileSystemType.Unknown;
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
	}
}
