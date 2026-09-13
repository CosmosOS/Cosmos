// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>
/// VFS inode backed by a FAT directory entry. Carries enough state to
/// rewrite the on-disk entry (size update, rename, delete) without a
/// fresh directory walk.
/// </summary>
internal sealed class FatInode : IVfsInode
{
    /// <summary>Sentinel <see cref="DirEntryByteOffset"/> for inodes with no backing on-disk 8.3 entry (the root directory).</summary>
    internal const int NoDirEntryOffset = -1;

    public FatSuperblock Superblock { get; }
    public string Name { get; internal set; }
    public FatAttr Attributes { get; internal set; }
    public uint FirstCluster { get; internal set; }
    public uint Size { get; internal set; }
    public FatInode? Parent { get; internal set; }

    /// <summary>Byte offset within parent directory data of the 8.3 entry that backs this inode; <c>-1</c> for the root.</summary>
    public int DirEntryByteOffset { get; internal set; } = NoDirEntryOffset;

    /// <summary>Number of contiguous 32-byte slots (LFN + 8.3) the on-disk entry occupies, for delete bookkeeping.</summary>
    public int DirEntrySlotCount { get; internal set; }

    public List<uint>? CachedChain { get; internal set; }

    internal FatInode(
        FatSuperblock superblock,
        string name,
        FatAttr attributes,
        uint firstCluster,
        uint size,
        FatInode? parent,
        int dirEntryByteOffset,
        int dirEntrySlotCount)
    {
        Superblock = superblock;
        Name = name;
        Attributes = attributes;
        FirstCluster = firstCluster;
        Size = size;
        Parent = parent;
        DirEntryByteOffset = dirEntryByteOffset;
        DirEntrySlotCount = dirEntrySlotCount;
    }

    public IInodeOperations InodeOperations => Superblock.InodeOps;

    public IFileOperations? FileOperations => IsDirectory ? null : Superblock.FileOps;

    public bool IsDirectory => (Attributes & FatAttr.Directory) != 0;

    public bool IsFixedRoot =>
        Parent is null && Superblock.Boot.Type != FatType.Fat32 && Superblock.Boot.RootSectorCount > 0;

    public List<uint> ResolveChain()
    {
        if (CachedChain is not null)
        {
            return CachedChain;
        }

        List<uint> chain = FirstCluster >= FatTable.FirstDataCluster ? Superblock.Fat.GetChain(FirstCluster) : [];
        CachedChain = chain;
        return chain;
    }

    public void InvalidateChain()
    {
        CachedChain = null;
    }
}
