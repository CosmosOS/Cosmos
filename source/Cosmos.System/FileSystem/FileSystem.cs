using System.Collections.Generic;
using System.IO;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem
{
    public abstract class FileSystem
    {
        public static FileSystemType GetFileSystemType(Partition aDevice)
        {
            if (FatFileSystem.IsDeviceFAT(aDevice))
            {
                return FileSystemType.FAT;
            }

            return FileSystemType.Unknown;
        }

        public abstract void DisplayFileSystemInfo();

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory);

        public abstract DirectoryEntry GetRootDirectory(string name);

        public abstract Stream GetFileStream(DirectoryEntry fileInfo);

        public abstract DirectoryEntry CreateDirectory(string aPath);
    }
}
