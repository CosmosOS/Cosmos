// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Per-mount filesystem instance.
/// </summary>
public interface IVfsSuperblock
{
    /// <summary>Root directory inode of this mount.</summary>
    IVfsInode Root { get; }

    /// <summary>Superblock-level callbacks (sync, statfs, unmount).</summary>
    ISuperblockOperations SuperOperations { get; }

    /// <summary>Fundamental block size in bytes, or 0 if not applicable.</summary>
    long BlockSize { get; }

    /// <summary>Maximum file name length (bytes).</summary>
    ulong MaxNameLength { get; }
}
