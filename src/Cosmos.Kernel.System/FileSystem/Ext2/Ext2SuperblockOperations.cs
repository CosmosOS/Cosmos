// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Ext2;

/// <summary>
/// Superblock callbacks for an ext2 mount: sync, statistics and teardown.
/// </summary>
internal sealed class Ext2SuperblockOperations : ISuperblockOperations
{
    /// <summary>Filesystem magic reported by statfs (s_magic).</summary>
    public const ulong Magic = 0xEF53u;

    public bool Sync(IVfsSuperblock superblock)
    {
        if (superblock is not Ext2Superblock ext2)
        {
            return false;
        }

        // Writes are durable only after Flush per the IBlockDevice
        // contract; sync is exactly that durability point.
        ext2.Flush();
        return true;
    }

    public bool StatFs(IVfsSuperblock superblock, out VfsStatFs statFs)
    {
        statFs = default;
        if (superblock is not Ext2Superblock ext2)
        {
            return false;
        }

        statFs.Type = Magic;
        statFs.BlockSize = ext2.BlockSize;
        statFs.Blocks = ext2.BlocksCount;
        statFs.FreeBlocks = ext2.FreeBlocksCount;
        statFs.AvailableBlocks = ext2.FreeBlocksCount;
        statFs.Inodes = ext2.InodesCount;
        statFs.FreeInodes = ext2.FreeInodesCount;
        statFs.MaxNameLength = ext2.MaxNameLength;
        statFs.FragmentSize = ext2.BlockSize;
        return true;
    }

    public void Drop(IVfsSuperblock superblock)
    {
        if (superblock is Ext2Superblock ext2)
        {
            // Unmount is a durability point: flush before tearing down.
            ext2.Flush();
            ext2.Drop();
        }
    }
}
