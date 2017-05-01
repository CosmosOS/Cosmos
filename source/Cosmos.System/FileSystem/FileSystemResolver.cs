using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;

namespace Cosmos.System.FileSystem
{
	public class FileSystemResolver
	{
		public virtual FileSystemType Resolve(Partition partition)
		{
			if (FatFileSystem.IsDeviceFat(partition)) return FileSystemType.FAT;
			return FileSystemType.Unknown;
		}
	}
}