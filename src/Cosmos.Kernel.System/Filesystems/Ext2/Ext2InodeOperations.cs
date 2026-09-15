// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;
using global::System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// Directory and inode metadata operations for an ext2 volume. Names match
/// case-sensitively; symlinks are created as real inodes and resolved by
/// the VFS layer.
/// </summary>
internal sealed class Ext2InodeOperations : IInodeOperations
{
    private readonly Ext2Superblock _superblock;

    /// <summary>
    /// Creates inode operations bound to a mounted volume.
    /// </summary>
    /// <param name="superblock">The mounted volume.</param>
    public Ext2InodeOperations(Ext2Superblock superblock)
    {
        _superblock = superblock;
    }

    /// <summary>
    /// Resolve a child entry by exact (case-sensitive) name. Dot entries
    /// resolve structurally without touching the inode cache.
    /// </summary>
    /// <param name="dir">Directory to search.</param>
    /// <param name="name">Child name, a single path component.</param>
    /// <param name="child">Resolved child inode, or null when not found.</param>
    /// <returns>true when the name exists in <paramref name="dir"/>.</returns>
    public bool Lookup(IVfsInode dir, ReadOnlySpan<char> name, [NotNullWhen(true)] out IVfsInode? child)
    {
        child = null;
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        string target = name.ToString();
        if (target == ".")
        {
            child = parent;
            return true;
        }

        if (target == "..")
        {
            // The root's parent is itself; other directories read the
            // parent inode number from their ".." entry.
            if (parent.InodeNumber == Ext2SuperblockLayout.RootInodeNumber)
            {
                child = parent;
                return true;
            }

            List<Ext2DirEntry> entries = Ext2DirectoryHelper.ParseDirectory(_superblock, parent);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Name == "..")
                {
                    uint ino = entries[i].Inode;
                    Ext2Inode found = _superblock.ReadInode(ino, "..");
                    child = found;
                    return true;
                }
            }

            // A directory without a ".." entry is corrupt; anchor at the
            // root rather than failing the lookup.
            child = _superblock.Root;
            return true;
        }

        if (!Ext2DirectoryHelper.TryFind(_superblock, parent, name, out Ext2DirEntry entry))
        {
            return false;
        }

        Ext2Inode node = _superblock.ReadInode(entry.Inode, target);
        child = node;
        return true;
    }

    /// <summary>
    /// List the directory's children, skipping the "." and ".." entries.
    /// </summary>
    /// <param name="dir">Directory to list.</param>
    /// <param name="entries">Child inodes on success; empty on failure.</param>
    /// <returns>true on success.</returns>
    public bool ReadDir(IVfsInode dir, out IReadOnlyList<IVfsInode> entries)
    {
        entries = Array.Empty<IVfsInode>();
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        List<Ext2DirEntry> raw = Ext2DirectoryHelper.ParseDirectory(_superblock, parent);
        List<IVfsInode> result = new(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            Ext2DirEntry e = raw[i];
            if (e.Name == "." || e.Name == "..")
            {
                continue;
            }

            Ext2Inode node = _superblock.ReadInode(e.Inode, e.Name);
            result.Add(node);
        }

        entries = result;
        return true;
    }

    /// <summary>
    /// Create an empty regular file in a directory.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new file, a single path component.</param>
    /// <param name="mode">Permission bits for the new inode.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the name already exists or allocation fails.</returns>
    public bool Create(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        // Duplicate names make an invalid volume (the second entry is
        // unreachable, fsck flags it).
        if (Ext2DirectoryHelper.TryFind(_superblock, parent, name, out _))
        {
            return false;
        }

        string strName = name.ToString();
        if (strName.Length == 0 || strName.Length > 255)
        {
            return false;
        }

        if (!_superblock.TryAllocateInode(_superblock.GroupOfInode(parent.InodeNumber), out uint newIno))
        {
            return false;
        }

        ushort ext2Mode = ToExt2Mode((mode & VfsMode.PermissionMask) | VfsMode.RegularFile);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Ext2Inode newNode = new(_superblock, newIno, strName)
        {
            Mode = ext2Mode,
            Uid = 0,
            Gid = 0,
            Size = 0,
            SizeHigh = 0,
            Atime = now,
            Ctime = now,
            Mtime = now,
            LinksCount = 1,
            Blocks = 0,
        };

        _superblock.WriteInode(newNode);

        byte ft = Ext2DirectoryHelper.FileTypeFromMode(ext2Mode);
        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, strName, newIno, ft))
        {
            _superblock.FreeInode(newIno);
            return false;
        }

        inode = newNode;
        return true;
    }

    /// <summary>
    /// Create an empty subdirectory holding its "." and ".." entries.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new directory, a single path component.</param>
    /// <param name="mode">Permission bits for the new inode.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the name already exists or allocation fails.</returns>
    public bool Mkdir(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (Ext2DirectoryHelper.TryFind(_superblock, parent, name, out _))
        {
            return false;
        }

        string strName = name.ToString();
        if (!_superblock.TryAllocateInode(_superblock.GroupOfInode(parent.InodeNumber), out uint newIno))
        {
            return false;
        }

        if (!_superblock.TryAllocateBlock(_superblock.GroupOfInode(parent.InodeNumber), out uint block))
        {
            _superblock.FreeInode(newIno);
            return false;
        }

        ushort ext2Mode = ToExt2Mode((mode & VfsMode.PermissionMask) | VfsMode.Directory);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Ext2Inode newNode = new(_superblock, newIno, strName)
        {
            Mode = ext2Mode,
            Uid = 0,
            Gid = 0,
            Atime = now,
            Ctime = now,
            Mtime = now,
            LinksCount = 2,
            Blocks = _superblock.BlockSize / 512,
            Block = new uint[15],
        };
        newNode.Block[0] = block;

        // A fresh directory holds "." (itself) and ".." (the parent).
        byte[] dirBlock = new byte[_superblock.BlockSize];
        BitConverter.TryWriteBytes(dirBlock.AsSpan(Ext2InodeLayout.DirEntryInodeOffset, 4), newIno);
        byte dotLen = 1;
        int dotRecLen = (Ext2InodeLayout.DirEntryNameOffset + dotLen + 3) & ~3;
        BitConverter.TryWriteBytes(dirBlock.AsSpan(Ext2InodeLayout.DirEntryRecLenOffset, 2), (ushort)dotRecLen);
        dirBlock[Ext2InodeLayout.DirEntryNameLenOffset] = dotLen;
        dirBlock[Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        dirBlock[Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        int dotDotPos = dotRecLen;
        BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryInodeOffset, 4), parent.InodeNumber);
        ushort dotDotRecLen = (ushort)(_superblock.BlockSize - dotRecLen);
        BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryRecLenOffset, 2), dotDotRecLen);
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameLenOffset] = 2;
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameOffset + 1] = (byte)'.';

        newNode.Size = _superblock.BlockSize;
        _superblock.WriteBlocks(block, 1, dirBlock);
        _superblock.WriteInode(newNode);

        // Add entry to parent.
        byte ft = Ext2InodeLayout.FileTypeDir;
        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, strName, newIno, ft))
        {
            _superblock.FreeBlock(block);
            _superblock.FreeInode(newIno);
            return false;
        }

        // Each subdirectory adds one link to its parent via "..".
        parent.LinksCount++;
        _superblock.WriteInode(parent);

        // The allocator tracks free counts only; persist the directory count
        // with the same group-descriptor write the superblock uses.
        uint group = _superblock.GroupOfInode(newIno);
        _superblock.GetGroup(group).UsedDirsCount++;
        _superblock.UpdateSuperblock();
        {
            uint gdStartBlock = _superblock.BlockSize == 1024 ? 2u : 1u;
            uint groups = _superblock.GroupsCount;
            uint gdBytes = groups * (uint)Ext2SuperblockLayout.GroupDescSize;
            uint gdBlocks = (gdBytes + _superblock.BlockSize - 1) / _superblock.BlockSize;
            byte[] gdBuf = new byte[gdBlocks * _superblock.BlockSize];
            _superblock.ReadBlocks(gdStartBlock, gdBlocks, gdBuf);
            int off = (int)group * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), _superblock.GetGroup(group).UsedDirsCount);
            _superblock.WriteBlocks(gdStartBlock, gdBlocks, gdBuf);
        }

        inode = newNode;
        return true;
    }

    /// <summary>
    /// Create a symbolic link pointing at <paramref name="target"/>.
    /// Short targets pack into i_block; longer ones use data blocks.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new link, a single path component.</param>
    /// <param name="target">Path the link points to; stored verbatim, not resolved.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the name already exists or allocation fails.</returns>
    public bool Symlink(IVfsInode dir, ReadOnlySpan<char> name, ReadOnlySpan<char> target, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (Ext2DirectoryHelper.TryFind(_superblock, parent, name, out _))
        {
            return false;
        }

        string strName = name.ToString();
        string strTarget = target.ToString();
        byte[] targetBytes = global::System.Text.Encoding.UTF8.GetBytes(strTarget);
        const int MaxSymlinkBlocks = 16;
        if (targetBytes.Length > _superblock.BlockSize * MaxSymlinkBlocks)
        {
            return false;
        }

        if (!_superblock.TryAllocateInode(_superblock.GroupOfInode(parent.InodeNumber), out uint newIno))
        {
            return false;
        }

        ushort ext2Mode = ToExt2Mode(VfsMode.SymbolicLink | VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.OwnerExecute | VfsMode.GroupRead | VfsMode.GroupExecute | VfsMode.OtherRead | VfsMode.OtherExecute);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Ext2Inode newNode = new(_superblock, newIno, strName)
        {
            Mode = ext2Mode,
            Uid = 0,
            Gid = 0,
            Atime = now,
            Ctime = now,
            Mtime = now,
            LinksCount = 1,
            Blocks = 0,
            Block = new uint[15],
        };

        if (targetBytes.Length <= Ext2InodeLayout.InlineSymlinkMax)
        {
            Ext2FileOperations.WriteInlineSymlink(newNode, strTarget);
        }
        else
        {
            uint blocksNeeded = (uint)((targetBytes.Length + _superblock.BlockSize - 1) / _superblock.BlockSize);
            for (uint i = 0; i < blocksNeeded; i++)
            {
                if (!_superblock.TryAllocateBlock(_superblock.GroupOfInode(newIno), out uint blk))
                {
                    // Cleanup prior blocks.
                    for (uint j = 0; j < i; j++)
                    {
                        uint b = newNode.Block[j];
                        if (b != 0)
                        {
                            _superblock.FreeBlock(b);
                        }
                    }

                    _superblock.FreeInode(newIno);
                    return false;
                }

                newNode.Block[i] = blk;
                newNode.Blocks += _superblock.BlockSize / 512;
                byte[] blkData = new byte[_superblock.BlockSize];
                int off = (int)i * (int)_superblock.BlockSize;
                int toCopy = Math.Min((int)_superblock.BlockSize, targetBytes.Length - off);
                targetBytes.AsSpan(off, toCopy).CopyTo(blkData.AsSpan(0, toCopy));
                _superblock.WriteBlocks(blk, 1, blkData);
            }

            newNode.Size = (uint)targetBytes.Length;
            newNode.SizeHigh = 0;
        }

        _superblock.WriteInode(newNode);

        byte ft = Ext2InodeLayout.FileTypeSymlink;
        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, strName, newIno, ft))
        {
            if (newNode.Blocks > 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (newNode.Block[i] != 0)
                    {
                        _superblock.FreeBlock(newNode.Block[i]);
                    }
                }
            }

            _superblock.FreeInode(newIno);
            return false;
        }

        inode = newNode;
        return true;
    }

    /// <summary>
    /// Remove the directory entry for a non-directory child and free its
    /// storage once the last link goes away.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the entry to remove.</param>
    /// <returns>true on success; false when the name does not exist or names a directory.</returns>
    public bool Unlink(IVfsInode dir, ReadOnlySpan<char> name)
    {
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        string strName = name.ToString();
        if (!Ext2DirectoryHelper.TryFind(_superblock, parent, name, out Ext2DirEntry entry))
        {
            return false;
        }

        Ext2Inode target = _superblock.ReadInode(entry.Inode, strName);
        if (target.IsDirectory)
        {
            return false;
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, parent, strName))
        {
            return false;
        }

        target.LinksCount--;
        if (target.LinksCount == 0)
        {
            _superblock.Truncate(target, 0);
            _superblock.FreeInode(target.InodeNumber);
        }
        else
        {
            _superblock.WriteInode(target);
        }

        return true;
    }

    /// <summary>
    /// Remove an empty child directory and release its block and inode.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the directory to remove.</param>
    /// <returns>true on success; false when the name does not exist, is not a directory, or is not empty.</returns>
    public bool Rmdir(IVfsInode dir, ReadOnlySpan<char> name)
    {
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        string strName = name.ToString();
        if (!Ext2DirectoryHelper.TryFind(_superblock, parent, name, out Ext2DirEntry entry))
        {
            return false;
        }

        Ext2Inode target = _superblock.ReadInode(entry.Inode, strName);
        if (!target.IsDirectory)
        {
            return false;
        }

        // Only "." and ".." may remain.
        List<Ext2DirEntry> entries = Ext2DirectoryHelper.ParseDirectory(_superblock, target);
        for (int i = 0; i < entries.Count; i++)
        {
            string n = entries[i].Name;
            if (n != "." && n != "..")
            {
                return false;
            }
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, parent, strName))
        {
            return false;
        }

        uint blk = target.Block[0];
        if (blk != 0)
        {
            _superblock.FreeBlock(blk);
        }

        target.LinksCount = 0;
        _superblock.FreeInode(target.InodeNumber);

        // Removing a subdirectory drops one parent link.
        parent.LinksCount--;
        _superblock.WriteInode(parent);

        uint group = _superblock.GroupOfInode(target.InodeNumber);
        Ext2GroupDesc gd = _superblock.GetGroup(group);
        if (gd.UsedDirsCount > 0)
        {
            gd.UsedDirsCount--;
        }

        {
            uint gdStartBlock = _superblock.BlockSize == 1024 ? 2u : 1u;
            uint groups = _superblock.GroupsCount;
            uint gdBytes = groups * (uint)Ext2SuperblockLayout.GroupDescSize;
            uint gdBlocks = (gdBytes + _superblock.BlockSize - 1) / _superblock.BlockSize;
            byte[] gdBuf = new byte[gdBlocks * _superblock.BlockSize];
            _superblock.ReadBlocks(gdStartBlock, gdBlocks, gdBuf);
            int off = (int)group * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), gd.UsedDirsCount);
            _superblock.WriteBlocks(gdStartBlock, gdBlocks, gdBuf);
        }

        return true;
    }

    /// <summary>
    /// Move an entry to a new parent and/or name. Both directories belong
    /// to the same filesystem instance.
    /// </summary>
    /// <param name="oldParent">Directory currently holding the entry.</param>
    /// <param name="oldName">Current name of the entry.</param>
    /// <param name="newParent">Destination directory; may equal <paramref name="oldParent"/>.</param>
    /// <param name="newName">New name for the entry.</param>
    /// <returns>true when the entry was moved.</returns>
    public bool Rename(IVfsInode oldParent, ReadOnlySpan<char> oldName, IVfsInode newParent, ReadOnlySpan<char> newName)
    {
        if (oldParent is not Ext2Inode op || newParent is not Ext2Inode np)
        {
            return false;
        }

        string sOld = oldName.ToString();
        string sNew = newName.ToString();

        if (!Ext2DirectoryHelper.TryFind(_superblock, op, oldName, out Ext2DirEntry oldEntry))
        {
            return false;
        }

        if (Ext2DirectoryHelper.TryFind(_superblock, np, newName, out Ext2DirEntry existing))
        {
            // Replace semantics are not implemented: refuse an existing
            // destination instead of writing a duplicate name.
            return false;
        }

        byte ft = oldEntry.FileType;
        if (ft == 0)
        {
            Ext2Inode node = _superblock.ReadInode(oldEntry.Inode, sOld);
            ft = Ext2DirectoryHelper.FileTypeFromMode(node.Mode);
        }

        if (!Ext2DirectoryHelper.AddEntry(_superblock, np, sNew, oldEntry.Inode, ft))
        {
            return false;
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, op, sOld))
        {
            Ext2DirectoryHelper.RemoveEntry(_superblock, np, sNew);
            return false;
        }

        // A directory moved across parents keeps a ".." pointing at the
        // old parent; rewrite it.
        if (ft == Ext2InodeLayout.FileTypeDir && !ReferenceEquals(op, np))
        {
            Ext2Inode moved = _superblock.ReadInode(oldEntry.Inode, sNew);
            uint blk = moved.Block[0];
            if (blk != 0)
            {
                byte[] dirBlock = new byte[_superblock.BlockSize];
                _superblock.ReadBlocks(blk, 1, dirBlock);
                int dotDotPos = (Ext2InodeLayout.DirEntryNameOffset + 1 + 3) & ~3;
                BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryInodeOffset, 4), np.InodeNumber);
                _superblock.WriteBlocks(blk, 1, dirBlock);
            }
        }

        return true;
    }

    /// <summary>
    /// Read the inode's attributes.
    /// </summary>
    /// <param name="inode">Inode to query.</param>
    /// <param name="stat">Populated attributes on success.</param>
    /// <returns>true on success.</returns>
    public bool GetAttr(IVfsInode inode, out VfsStat stat)
    {
        stat = default;
        if (inode is not Ext2Inode node)
        {
            return false;
        }

        stat.Ino = node.InodeNumber;
        stat.Mode = ToVfsMode(node.Mode);
        stat.NLink = node.LinksCount;
        stat.Uid = node.Uid;
        stat.Gid = node.Gid;
        stat.Rdev = 0;
        stat.Size = node.FullSize;
        stat.BlkSize = _superblock.BlockSize;
        stat.Blocks = node.Blocks;
        stat.Atime = new VfsTimespec(node.Atime, 0);
        stat.Mtime = new VfsTimespec(node.Mtime, 0);
        stat.Ctime = new VfsTimespec(node.Ctime, 0);
        return true;
    }

    /// <summary>
    /// Update the inode attributes selected by <paramref name="flags"/>.
    /// Selecting <see cref="SetAttrFlags.Size"/> truncates or zero-extends
    /// the file; fields without a corresponding flag are ignored.
    /// </summary>
    /// <param name="inode">Inode to modify.</param>
    /// <param name="flags">Which fields of <paramref name="attributes"/> to apply.</param>
    /// <param name="attributes">Source values for the selected fields.</param>
    /// <returns>true on success; false when a selected change is not supported.</returns>
    public bool SetAttr(IVfsInode inode, SetAttrFlags flags, in VfsStat attributes)
    {
        if (inode is not Ext2Inode node)
        {
            return false;
        }

        if ((flags & SetAttrFlags.Size) != 0)
        {
            if (node.IsDirectory)
            {
                return false;
            }

            ulong newSize = attributes.Size;
            if (newSize < node.FullSize)
            {
                _superblock.Truncate(node, newSize);
            }
            else if (newSize > node.FullSize)
            {
                // Growing records the size; the blocks materialize on the
                // next write, and the gap reads as zeros until then.
                node.Size = (uint)(newSize & 0xFFFFFFFF);
                node.SizeHigh = (uint)(newSize >> 32);
                _superblock.WriteInode(node);
            }
        }

        if ((flags & SetAttrFlags.Mode) != 0)
        {
            // The file type is immutable; only permission bits change.
            ushort curType = (ushort)(node.Mode & Ext2InodeLayout.IFMT);
            ushort newPerms = (ushort)(ToExt2Mode(attributes.Mode) & 0x0FFF);
            node.Mode = (ushort)(curType | newPerms);
        }

        if ((flags & SetAttrFlags.Uid) != 0)
        {
            node.Uid = (ushort)attributes.Uid;
        }

        if ((flags & SetAttrFlags.Gid) != 0)
        {
            node.Gid = (ushort)attributes.Gid;
        }

        if ((flags & SetAttrFlags.Atime) != 0)
        {
            node.Atime = (uint)attributes.Atime.TvSec;
        }

        if ((flags & SetAttrFlags.Mtime) != 0)
        {
            node.Mtime = (uint)attributes.Mtime.TvSec;
        }

        if ((flags & SetAttrFlags.Ctime) != 0)
        {
            node.Ctime = (uint)attributes.Ctime.TvSec;
        }

        _superblock.WriteInode(node);
        return true;
    }

    /// <summary>
    /// Read the target of a symbolic link.
    /// </summary>
    /// <param name="symlink">Inode of the symbolic link.</param>
    /// <param name="target">Link target on success.</param>
    /// <returns>true when <paramref name="symlink"/> is a symbolic link and the target was read.</returns>
    public bool TryReadLink(IVfsInode symlink, out string? target)
    {
        target = null;
        if (symlink is not Ext2Inode node || !node.IsSymlink)
        {
            return false;
        }

        target = Ext2FileOperations.ReadSymlinkTarget(node);
        return true;
    }

    /// <summary>
    /// Convert a VFS mode to raw i_mode bits (file type plus the low 12 permission bits).
    /// </summary>
    /// <param name="mode">VFS mode.</param>
    private static ushort ToExt2Mode(VfsMode mode)
    {
        ushort m = 0;
        VfsMode type = mode & VfsMode.FileTypeMask;
        m |= type switch
        {
            VfsMode.RegularFile => Ext2InodeLayout.IFREG,
            VfsMode.Directory => Ext2InodeLayout.IFDIR,
            VfsMode.SymbolicLink => Ext2InodeLayout.IFLNK,
            VfsMode.CharacterDevice => Ext2InodeLayout.IFCHR,
            VfsMode.BlockDevice => Ext2InodeLayout.IFBLK,
            VfsMode.NamedPipe => Ext2InodeLayout.IFIFO,
            VfsMode.Socket => Ext2InodeLayout.IFSOCK,
            _ => (ushort)0,
        };

        // Permission bits occupy the same low 12 bits in both encodings.
        m |= (ushort)((uint)mode & 0x0FFF);
        return m;
    }

    /// <summary>
    /// Convert raw i_mode bits to a VFS mode.
    /// </summary>
    /// <param name="ext2Mode">Raw i_mode value.</param>
    private static VfsMode ToVfsMode(ushort ext2Mode)
    {
        VfsMode m = 0;
        ushort type = (ushort)(ext2Mode & Ext2InodeLayout.IFMT);
        m |= type switch
        {
            Ext2InodeLayout.IFREG => VfsMode.RegularFile,
            Ext2InodeLayout.IFDIR => VfsMode.Directory,
            Ext2InodeLayout.IFLNK => VfsMode.SymbolicLink,
            Ext2InodeLayout.IFCHR => VfsMode.CharacterDevice,
            Ext2InodeLayout.IFBLK => VfsMode.BlockDevice,
            Ext2InodeLayout.IFIFO => VfsMode.NamedPipe,
            Ext2InodeLayout.IFSOCK => VfsMode.Socket,
            _ => 0,
        };

        m |= (VfsMode)(ext2Mode & 0x0FFF);
        return m;
    }
}
