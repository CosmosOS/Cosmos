// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Inode attributes for <c>getattr</c> / <c>stat</c>.
/// </summary>
public struct VfsStat
{
    /// <summary>Inode number, unique within the filesystem instance (<c>st_ino</c>).</summary>
    public ulong Ino;
    /// <summary>File type and permission bits (<c>st_mode</c>).</summary>
    public VfsMode Mode;
    /// <summary>Number of hard links to the inode (<c>st_nlink</c>).</summary>
    public uint NLink;
    /// <summary>Owner user id (<c>st_uid</c>).</summary>
    public uint Uid;
    /// <summary>Owner group id (<c>st_gid</c>).</summary>
    public uint Gid;
    /// <summary>Device id for character and block device nodes; 0 otherwise (<c>st_rdev</c>).</summary>
    public ulong Rdev;
    /// <summary>File size in bytes (<c>st_size</c>).</summary>
    public ulong Size;
    /// <summary>Preferred I/O block size in bytes (<c>st_blksize</c>).</summary>
    public long BlkSize;
    /// <summary>Number of storage blocks allocated to the inode (<c>st_blocks</c>).</summary>
    public ulong Blocks;
    /// <summary>Last access time (<c>st_atim</c>).</summary>
    public VfsTimespec Atime;
    /// <summary>Last data modification time (<c>st_mtim</c>).</summary>
    public VfsTimespec Mtime;
    /// <summary>Last status change time (<c>st_ctim</c>).</summary>
    public VfsTimespec Ctime;

    /// <summary>True when <see cref="Mode"/> encodes a directory.</summary>
    /// <remarks>
    /// The file-type nibble is an enumerated field, not a bit set. Test it with
    /// this helper, never with <c>Mode.HasFlag(VfsMode.Directory)</c>, which is
    /// also true for <see cref="VfsMode.BlockDevice"/> and
    /// <see cref="VfsMode.Socket"/>.
    /// </remarks>
    public readonly bool IsDirectory => (Mode & VfsMode.FileTypeMask) == VfsMode.Directory;

    /// <summary>True when <see cref="Mode"/> encodes a regular file.</summary>
    /// <remarks>See <see cref="IsDirectory"/> for why this is not a flag test.</remarks>
    public readonly bool IsRegularFile => (Mode & VfsMode.FileTypeMask) == VfsMode.RegularFile;

    /// <summary>True when <see cref="Mode"/> encodes a symbolic link.</summary>
    /// <remarks>See <see cref="IsDirectory"/> for why this is not a flag test.</remarks>
    public readonly bool IsSymbolicLink => (Mode & VfsMode.FileTypeMask) == VfsMode.SymbolicLink;
}
