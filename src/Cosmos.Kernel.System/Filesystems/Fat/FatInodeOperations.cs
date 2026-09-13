// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Fat;

internal sealed class FatInodeOperations : IInodeOperations
{
    /// <summary>Byte offset of the '.' entry — the first slot of a new directory's cluster (fatgen103 §6).</summary>
    private const int DotEntryOffset = 0;

    /// <summary>Byte offset of the '..' entry — the second directory slot, right after '.' (fatgen103 §6).</summary>
    private const int DotDotEntryOffset = FatDirectory.EntrySize;

    /// <summary>First-cluster value the '..' entry stores when the parent is the root directory (fatgen103 §6).</summary>
    private const uint RootDotDotCluster = 0u;

    /// <summary>DIR_FileSize recorded for a newly created, still-empty file (bytes).</summary>
    private const uint EmptyFileSize = 0u;

    /// <summary>Cluster count a fresh directory starts with — one cluster holding its '.'/'..' entries.</summary>
    private const uint InitialDirectoryClusters = 1u;

    /// <summary>Link count every FAT inode reports — FAT has no hard links.</summary>
    private const uint FatLinkCount = 1u;

    private readonly FatSuperblock _superblock;

    public FatInodeOperations(FatSuperblock superblock)
    {
        _superblock = superblock;
    }

    public bool Lookup(IVfsInode dir, ReadOnlySpan<char> name, [NotNullWhen(true)] out IVfsInode? child)
    {
        child = null;
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        string targetName = name.ToString();
        // Dot entries resolve structurally: routing them through
        // GetOrCreateInode would hijack the cached parent/self inode
        // (the cache is keyed by first cluster), re-pointing its name,
        // parent and directory-slot reference at the dot entry.
        if (targetName == ".")
        {
            child = parent;
            return true;
        }
        if (targetName == "..")
        {
            child = parent.Parent ?? parent;
            return true;
        }

        byte[] data = _superblock.ReadDirectoryData(parent);
        List<FatDirEntry> entries = FatDirectory.Parse(data, _superblock.Boot.Type == FatType.Fat32);

        for (int i = 0; i < entries.Count; i++)
        {
            FatDirEntry entry = entries[i];
            if (entry.IsVolumeId || entry.Name == "." || entry.Name == "..")
            {
                continue;
            }

            if (FatDirectory.NameEqualsIgnoreCase(entry.Name, targetName) || FatDirectory.NameEqualsIgnoreCase(entry.ShortName, targetName))
            {
                FatInode found = _superblock.GetOrCreateInode(parent, entry);
                child = found;
                return true;
            }
        }

        return false;
    }

    public bool ReadDir(IVfsInode dir, out IReadOnlyList<IVfsInode> entries)
    {
        entries = Array.Empty<IVfsInode>();
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        byte[] data = _superblock.ReadDirectoryData(parent);
        List<FatDirEntry> raw = FatDirectory.Parse(data, _superblock.Boot.Type == FatType.Fat32);
        List<IVfsInode> result = new(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            FatDirEntry entry = raw[i];
            if (entry.IsVolumeId)
            {
                continue;
            }
            if (entry.Name == "." || entry.Name == "..")
            {
                continue;
            }

            result.Add(_superblock.GetOrCreateInode(parent, entry));
        }
        entries = result;
        return true;
    }

    public bool Create(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        // Duplicate names make an invalid volume (the second entry is
        // unreachable, chkdsk flags it).
        if (_superblock.FindChildEntry(parent, name, out _))
        {
            return false;
        }

        FatAttr attr = FatAttributes.ToFatAttr(mode) & ~FatAttr.Directory;
        if (!_superblock.AllocateDirectoryEntry(parent, name, attr, FatDirectory.EmptyFirstCluster, EmptyFileSize, out FatInode? created))
        {
            return false;
        }

        inode = created;
        return true;
    }

    public bool Mkdir(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (_superblock.FindChildEntry(parent, name, out _))
        {
            return false;
        }

        uint cluster = _superblock.Fat.AllocateChain(InitialDirectoryClusters);
        if (cluster == 0)
        {
            return false;
        }

        Span<byte> clusterBuffer = new byte[_superblock.Boot.BytesPerCluster];
        // fatgen103: '..' stores 0 when the parent is the root directory
        // (the FAT32 root has a real cluster number, but '..' must not).
        uint dotDotCluster = parent.Parent == null ? RootDotDotCluster : parent.FirstCluster;
        WriteDotEntries(clusterBuffer, cluster, dotDotCluster);
        _superblock.WriteCluster(cluster, clusterBuffer);

        FatAttr attr = FatAttributes.ToFatAttr(mode) | FatAttr.Directory;
        if (!_superblock.AllocateDirectoryEntry(parent, name, attr, cluster, FatDirectory.DirectorySizeOnDisk, out FatInode? created))
        {
            _superblock.Fat.Free(cluster);
            return false;
        }

        inode = created;
        return true;
    }

    public bool Symlink(IVfsInode dir, ReadOnlySpan<char> name, ReadOnlySpan<char> target, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        return false;
    }

    public bool Unlink(IVfsInode dir, ReadOnlySpan<char> name)
    {
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (!_superblock.FindChildEntry(parent, name, out FatDirEntry? match) || match == null)
        {
            return false;
        }

        if ((match.Attributes & FatAttr.Directory) != 0)
        {
            return false;
        }

        // Remove (and persist) the entry before freeing the chain: a
        // crash between the two steps then only leaks clusters, instead
        // of leaving a live entry pointing at clusters the next
        // allocation will reuse (cross-link).
        _superblock.RemoveDirectoryEntry(parent, match);
        if (match.FirstCluster >= FatTable.FirstDataCluster)
        {
            _superblock.Fat.Free(match.FirstCluster);
        }
        _superblock.ForgetInode(match.FirstCluster);
        return true;
    }

    public bool Rmdir(IVfsInode dir, ReadOnlySpan<char> name)
    {
        if (dir is not FatInode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (!_superblock.FindChildEntry(parent, name, out FatDirEntry? match) || match == null)
        {
            return false;
        }

        if ((match.Attributes & FatAttr.Directory) == 0)
        {
            return false;
        }

        FatInode target = _superblock.GetOrCreateInode(parent, match);
        if (!_superblock.IsDirectoryEmpty(target))
        {
            return false;
        }

        // Same ordering as Unlink: entry first, chain second.
        _superblock.RemoveDirectoryEntry(parent, match);
        if (target.FirstCluster >= FatTable.FirstDataCluster)
        {
            _superblock.Fat.Free(target.FirstCluster);
        }
        _superblock.ForgetInode(match.FirstCluster);
        return true;
    }

    public bool Rename(IVfsInode oldParent, ReadOnlySpan<char> oldName, IVfsInode newParent, ReadOnlySpan<char> newName)
    {
        if (oldParent is not FatInode op || newParent is not FatInode np)
        {
            return false;
        }

        if (!_superblock.FindChildEntry(op, oldName, out FatDirEntry? match) || match == null)
        {
            return false;
        }

        // Replace semantics are not implemented: refuse an existing
        // destination instead of writing a duplicate name.
        if (_superblock.FindChildEntry(np, newName, out _))
        {
            return false;
        }

        FatAttr attr = match.Attributes;
        uint firstCluster = match.FirstCluster;
        uint size = match.Size;

        // Allocate the destination first: a failed rename must leave the
        // filesystem unchanged (the allocator only fills free slots, so
        // the source entry stays valid until removed). Crash-safe too —
        // worst case is a transiently duplicated name, not a lost file.
        if (!_superblock.AllocateDirectoryEntry(np, newName, attr, firstCluster, size, out _))
        {
            return false;
        }

        _superblock.RemoveDirectoryEntry(op, match);
        _superblock.ForgetInode(match.FirstCluster);

        // A directory moved across parents keeps a '..' pointing at the
        // old parent; rewrite it (0 when the new parent is the root).
        if ((attr & FatAttr.Directory) != 0 && !ReferenceEquals(op, np)
            && firstCluster >= FatTable.FirstDataCluster)
        {
            RewriteDotDot(firstCluster, np);
        }

        return true;
    }

    /// <summary>Rewrites the '..' entry (slot 1) of the directory rooted at <paramref name="dirCluster"/>.</summary>
    private void RewriteDotDot(uint dirCluster, FatInode newParent)
    {
        uint parentCluster = newParent.Parent == null ? RootDotDotCluster : newParent.FirstCluster;
        Span<byte> clusterBuffer = new byte[_superblock.Boot.BytesPerCluster];
        _superblock.ReadCluster(dirCluster, clusterBuffer);
        int offset = DotDotEntryOffset;
        BitConverter.TryWriteBytes(
            clusterBuffer.Slice(offset + FatDirectory.FirstClusterHighOffset, FatDirectory.ClusterWordBytes),
            (ushort)((parentCluster >> FatDirectory.ClusterHighShift) & FatDirectory.ClusterWordMask));
        BitConverter.TryWriteBytes(
            clusterBuffer.Slice(offset + FatDirectory.FirstClusterLowOffset, FatDirectory.ClusterWordBytes),
            (ushort)(parentCluster & FatDirectory.ClusterWordMask));
        _superblock.WriteCluster(dirCluster, clusterBuffer);
    }

    public bool GetAttr(IVfsInode inode, out VfsStat stat)
    {
        stat = default;
        if (inode is not FatInode node)
        {
            return false;
        }

        stat.Ino = node.FirstCluster;
        stat.Mode = FatAttributes.ToMode(node.Attributes);
        stat.NLink = FatLinkCount;
        stat.Uid = 0;
        stat.Gid = 0;
        stat.Rdev = 0;
        stat.Size = node.Size;
        stat.BlkSize = _superblock.BlockSize;
        stat.Blocks = (ulong)node.ResolveChain().Count * _superblock.Boot.SectorsPerCluster;
        stat.Atime = new VfsTimespec(0, 0);
        stat.Mtime = new VfsTimespec(0, 0);
        stat.Ctime = new VfsTimespec(0, 0);
        return true;
    }

    public bool SetAttr(IVfsInode inode, SetAttrFlags flags, in VfsStat attributes)
    {
        if (inode is not FatInode node)
        {
            return false;
        }

        if ((flags & SetAttrFlags.Size) != 0)
        {
            // FAT caps file size at uint.MaxValue (a 4 GiB request would
            // wrap the cast to 0 and truncate the file), and directories
            // carry size 0 on disk — resizing one would wipe its live
            // clusters through the grow path.
            if (node.IsDirectory || attributes.Size > uint.MaxValue)
            {
                return false;
            }
            uint newSize = (uint)attributes.Size;
            if (newSize < node.Size)
            {
                _superblock.TruncateChain(node, newSize);
                node.Size = newSize;
            }
            else if (newSize > node.Size)
            {
                if (!_superblock.GrowZeroFilled(node, newSize))
                {
                    return false;
                }
            }
        }

        if ((flags & SetAttrFlags.Mode) != 0)
        {
            FatAttr cleared = FatAttributes.ToFatAttr(attributes.Mode);
            FatAttr preservedDir = node.Attributes & FatAttr.Directory;
            node.Attributes = cleared | preservedDir;
        }

        _superblock.UpdateInodeEntry(node);
        return true;
    }


    private static void WriteDotEntries(Span<byte> clusterBuffer, uint selfCluster, uint parentCluster)
    {
        clusterBuffer.Clear();
        FatDirectory.WriteShortEntry(clusterBuffer, DotEntryOffset, ".          ", FatAttr.Directory, selfCluster, FatDirectory.DirectorySizeOnDisk);
        FatDirectory.WriteShortEntry(clusterBuffer, DotDotEntryOffset, "..         ", FatAttr.Directory, parentCluster, FatDirectory.DirectorySizeOnDisk);
    }
}
