// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem;

/// <summary>
/// Filesystem statistics.
/// </summary>
public struct VfsStatFs
{
    /// <summary>Filesystem type magic number (<c>f_type</c>).</summary>
    public ulong Type;
    /// <summary>Block size in bytes; the unit for <see cref="Blocks"/>, <see cref="FreeBlocks"/> and <see cref="AvailableBlocks"/> (<c>f_bsize</c>).</summary>
    public ulong BlockSize;
    /// <summary>Total data blocks in the filesystem (<c>f_blocks</c>).</summary>
    public ulong Blocks;
    /// <summary>Free blocks (<c>f_bfree</c>).</summary>
    public ulong FreeBlocks;
    /// <summary>Free blocks available to unprivileged callers (<c>f_bavail</c>).</summary>
    public ulong AvailableBlocks;
    /// <summary>Total inodes, or 0 when the filesystem does not track an inode count (<c>f_files</c>).</summary>
    public ulong Inodes;
    /// <summary>Free inodes, or 0 when the filesystem does not track an inode count (<c>f_ffree</c>).</summary>
    public ulong FreeInodes;
    /// <summary>Maximum file name length in bytes (<c>f_namelen</c>).</summary>
    public ulong MaxNameLength;
    /// <summary>Fragment size in bytes (<c>f_frsize</c>).</summary>
    public ulong FragmentSize;
}
