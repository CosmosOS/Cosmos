// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Fat;

/// <summary>
/// Superblock callbacks for a FAT mount: sync, statistics and teardown.
/// </summary>
internal sealed class FatSuperblockOperations : ISuperblockOperations
{
    /// <summary>Filesystem magic reported by statfs.</summary>
    public const ulong Magic = 0x4D73_4154_4146u;

    public bool Sync(IVfsSuperblock superblock)
    {
        if (superblock is not FatSuperblock fat)
        {
            return false;
        }
        // Writes are durable only after Flush per the IBlockDevice
        // contract; sync is exactly that durability point.
        fat.Flush();
        return true;
    }

    public bool StatFs(IVfsSuperblock superblock, out VfsStatFs statFs)
    {
        statFs = default;
        if (superblock is not FatSuperblock fat)
        {
            return false;
        }

        FatBootSector boot = fat.Boot;
        statFs.Type = Magic;
        statFs.BlockSize = boot.BytesPerCluster;
        statFs.Blocks = boot.ClusterCount;
        statFs.FreeBlocks = fat.Fat.CountFree();
        statFs.AvailableBlocks = statFs.FreeBlocks;
        statFs.Inodes = 0;
        statFs.FreeInodes = 0;
        statFs.MaxNameLength = fat.MaxNameLength;
        statFs.FragmentSize = boot.BytesPerSector;
        return true;
    }

    public void Drop(IVfsSuperblock superblock)
    {
        if (superblock is FatSuperblock fat)
        {
            // Unmount is a durability point: flush before tearing down.
            fat.Flush();
            fat.Drop();
        }
    }
}
