// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem;

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

    /// <summary>
    /// True when the driver cannot write this volume, whatever flags it was
    /// mounted with: a CD-ROM image, a medium the driver only reads. The VFS
    /// then refuses every mutation under the mount, and <c>System.IO</c>
    /// reports them as a read-only file system. A driver that writes leaves
    /// the default.
    /// </summary>
    bool IsReadOnly => false;
}
