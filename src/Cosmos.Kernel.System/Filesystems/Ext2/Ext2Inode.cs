// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// VFS inode backed by an ext2 inode. Carries the parsed on-disk fields so
/// metadata updates (size, mode, link count) persist without a fresh inode
/// table read.
/// </summary>
internal sealed class Ext2Inode : IVfsInode
{
    /// <summary>The mounted volume owning this inode.</summary>
    public Ext2Superblock Superblock { get; }

    /// <summary>One-based inode number, unique within the volume.</summary>
    public uint InodeNumber { get; }

    /// <summary>Leaf name within the parent directory; updated on re-lookup.</summary>
    public string Name { get; internal set; }

    /// <summary>File type plus permission bits (i_mode).</summary>
    public ushort Mode { get; internal set; }

    /// <summary>Owner user id (i_uid).</summary>
    public ushort Uid { get; internal set; }

    /// <summary>Owner group id (i_gid).</summary>
    public ushort Gid { get; internal set; }

    /// <summary>File size in bytes, low 32 bits (i_size).</summary>
    public uint Size { get; internal set; }

    /// <summary>File size in bytes, high 32 bits (i_dir_acl for regular files).</summary>
    public uint SizeHigh { get; internal set; }

    /// <summary>Full 64-bit file size.</summary>
    public ulong FullSize => ((ulong)SizeHigh << 32) | Size;

    /// <summary>Last access time, Unix seconds (i_atime).</summary>
    public uint Atime { get; internal set; }

    /// <summary>Inode change time, Unix seconds (i_ctime).</summary>
    public uint Ctime { get; internal set; }

    /// <summary>Last modification time, Unix seconds (i_mtime).</summary>
    public uint Mtime { get; internal set; }

    /// <summary>Deletion time, Unix seconds (i_dtime).</summary>
    public uint Dtime { get; internal set; }

    /// <summary>Hard link count (i_links_count).</summary>
    public ushort LinksCount { get; internal set; }

    /// <summary>512-byte sectors allocated to the inode (i_blocks).</summary>
    public uint Blocks { get; internal set; }

    /// <summary>Inode flags (i_flags).</summary>
    public uint Flags { get; internal set; }

    /// <summary>15 block pointers: 12 direct, then single, double and triple indirect.</summary>
    public uint[] Block { get; internal set; } = new uint[Ext2InodeLayout.BlockCount];

    /// <summary>
    /// Creates an inode handle.
    /// </summary>
    /// <param name="superblock">The mounted volume owning the inode.</param>
    /// <param name="inodeNumber">One-based inode number.</param>
    /// <param name="name">Leaf name within the parent directory.</param>
    public Ext2Inode(Ext2Superblock superblock, uint inodeNumber, string name)
    {
        Superblock = superblock;
        InodeNumber = inodeNumber;
        Name = name;
    }

    public IInodeOperations InodeOperations => Superblock.InodeOps;

    public IFileOperations? FileOperations
    {
        get
        {
            ushort type = (ushort)(Mode & Ext2InodeLayout.IFMT);
            // Directories have no file ops; reads go through ReadDir.
            if (type == Ext2InodeLayout.IFDIR)
            {
                return null;
            }

            // Regular files and symlinks (for reading the target) are readable.
            if (type == Ext2InodeLayout.IFREG || type == Ext2InodeLayout.IFLNK)
            {
                return Superblock.FileOps;
            }

            return null;
        }
    }

    /// <summary>True when the mode encodes a directory.</summary>
    public bool IsDirectory => (Mode & Ext2InodeLayout.IFMT) == Ext2InodeLayout.IFDIR;

    /// <summary>True when the mode encodes a symbolic link.</summary>
    public bool IsSymlink => (Mode & Ext2InodeLayout.IFMT) == Ext2InodeLayout.IFLNK;

    /// <summary>True when the mode encodes a regular file.</summary>
    public bool IsRegularFile => (Mode & Ext2InodeLayout.IFMT) == Ext2InodeLayout.IFREG;
}
