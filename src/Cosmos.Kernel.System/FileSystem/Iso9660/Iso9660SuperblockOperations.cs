// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Superblock callbacks for an ISO 9660 mount: nothing to sync, the
/// statistics of a volume with no free space, and the teardown.
/// </summary>
internal sealed class Iso9660SuperblockOperations : ISuperblockOperations
{
    /// <summary>Filesystem magic reported by statfs: Linux's ISOFS_SUPER_MAGIC.</summary>
    public const ulong Magic = 0x9660u;

    /// <inheritdoc />
    public bool Sync(IVfsSuperblock superblock)
    {
        // A read-only volume has nothing to make durable.
        return superblock is Iso9660Superblock;
    }

    /// <inheritdoc />
    public bool StatFs(IVfsSuperblock superblock, out VfsStatFs statFs)
    {
        statFs = default;
        if (superblock is not Iso9660Superblock iso)
        {
            return false;
        }

        statFs.Type = Magic;
        statFs.BlockSize = iso.BlockSize;
        statFs.Blocks = iso.VolumeBlocks;
        statFs.FreeBlocks = 0;
        statFs.AvailableBlocks = 0;
        statFs.Inodes = 0;
        statFs.FreeInodes = 0;
        statFs.MaxNameLength = iso.MaxNameLength;
        statFs.FragmentSize = iso.BlockSize;
        return true;
    }

    /// <inheritdoc />
    public void Drop(IVfsSuperblock superblock)
    {
        if (superblock is Iso9660Superblock iso)
        {
            iso.Drop();
        }
    }
}
