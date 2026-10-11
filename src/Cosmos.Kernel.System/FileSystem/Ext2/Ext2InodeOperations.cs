// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using global::System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.System.FileSystem.Ext2;

/// <summary>
/// Directory and inode metadata operations for an ext2 volume. Names match
/// case-sensitively; symlinks are created as real inodes and resolved by
/// the VFS layer.
/// </summary>
internal sealed class Ext2InodeOperations : IInodeOperations
{
    /// <summary>Longest entry name: a directory entry stores the name length in one byte.</summary>
    private const int MaxNameLength = 255;

    /// <summary>Unit of i_blocks: 512-byte sectors, whatever the block size.</summary>
    private const uint SectorSize = 512;

    /// <summary>Low 12 bits of i_mode: the permission bits plus setuid, setgid and sticky.</summary>
    private const ushort ModeLowBits = 0x0FFF;

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
                    child = _superblock.ReadInode(entries[i].Inode, "..");
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

        child = _superblock.ReadInode(entry.Inode, target);
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
        entries = [];
        if (dir is not Ext2Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        List<Ext2DirEntry> rawEntries = Ext2DirectoryHelper.ParseDirectory(_superblock, parent);
        List<IVfsInode> result = new(rawEntries.Count);
        for (int i = 0; i < rawEntries.Count; i++)
        {
            Ext2DirEntry entry = rawEntries[i];
            if (entry.Name == "." || entry.Name == "..")
            {
                continue;
            }

            Ext2Inode node = _superblock.ReadInode(entry.Inode, entry.Name);
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

        string childName = name.ToString();
        if (childName.Length == 0 || childName.Length > MaxNameLength)
        {
            return false;
        }

        if (!_superblock.TryAllocateInode(_superblock.GroupOfInode(parent.InodeNumber), out uint newIno))
        {
            return false;
        }

        ushort ext2Mode = ToExt2Mode((mode & VfsMode.PermissionMask) | VfsMode.RegularFile);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Ext2Inode newNode = new(_superblock, newIno, childName)
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

        byte fileType = Ext2DirectoryHelper.FileTypeFromMode(ext2Mode);
        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, childName, newIno, fileType))
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

        string childName = name.ToString();
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
        Ext2Inode newNode = new(_superblock, newIno, childName)
        {
            Mode = ext2Mode,
            Uid = 0,
            Gid = 0,
            Atime = now,
            Ctime = now,
            Mtime = now,
            LinksCount = 2,
            Blocks = _superblock.BlockSize / SectorSize,
            Block = new uint[Ext2InodeLayout.BlockCount],
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

        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, childName, newIno, Ext2InodeLayout.FileTypeDir))
        {
            _superblock.FreeBlock(block);
            _superblock.FreeInode(newIno);
            return false;
        }

        // Each subdirectory adds one link to its parent via "..".
        parent.LinksCount++;
        _superblock.WriteInode(parent);

        uint group = _superblock.GroupOfInode(newIno);
        _superblock.GetGroup(group).UsedDirsCount++;
        _superblock.UpdateSuperblock();
        WriteUsedDirsCount(group);

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

        string childName = name.ToString();
        string targetPath = target.ToString();
        byte[] targetBytes = global::System.Text.Encoding.UTF8.GetBytes(targetPath);
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
        Ext2Inode newNode = new(_superblock, newIno, childName)
        {
            Mode = ext2Mode,
            Uid = 0,
            Gid = 0,
            Atime = now,
            Ctime = now,
            Mtime = now,
            LinksCount = 1,
            Blocks = 0,
            Block = new uint[Ext2InodeLayout.BlockCount],
        };

        if (targetBytes.Length <= Ext2InodeLayout.InlineSymlinkMax)
        {
            Ext2FileOperations.WriteInlineSymlink(newNode, targetPath);
        }
        else
        {
            uint blocksNeeded = (uint)((targetBytes.Length + _superblock.BlockSize - 1) / _superblock.BlockSize);
            for (uint i = 0; i < blocksNeeded; i++)
            {
                if (!_superblock.TryAllocateBlock(_superblock.GroupOfInode(newIno), out uint block))
                {
                    for (uint j = 0; j < i; j++)
                    {
                        uint allocated = newNode.Block[j];
                        if (allocated != 0)
                        {
                            _superblock.FreeBlock(allocated);
                        }
                    }

                    _superblock.FreeInode(newIno);
                    return false;
                }

                newNode.Block[i] = block;
                newNode.Blocks += _superblock.BlockSize / SectorSize;
                byte[] blockData = new byte[_superblock.BlockSize];
                int offset = (int)i * (int)_superblock.BlockSize;
                int toCopy = Math.Min((int)_superblock.BlockSize, targetBytes.Length - offset);
                targetBytes.AsSpan(offset, toCopy).CopyTo(blockData.AsSpan(0, toCopy));
                _superblock.WriteBlocks(block, 1, blockData);
            }

            newNode.Size = (uint)targetBytes.Length;
            newNode.SizeHigh = 0;
        }

        _superblock.WriteInode(newNode);

        if (!Ext2DirectoryHelper.AddEntry(_superblock, parent, childName, newIno, Ext2InodeLayout.FileTypeSymlink))
        {
            // An inline target fills i_block with the path bytes, not block
            // numbers; only a link with data blocks has any to free.
            if (newNode.Blocks > 0)
            {
                for (int i = 0; i < Ext2InodeLayout.BlockCount; i++)
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

        string childName = name.ToString();
        if (!Ext2DirectoryHelper.TryFind(_superblock, parent, name, out Ext2DirEntry entry))
        {
            return false;
        }

        Ext2Inode target = _superblock.ReadInode(entry.Inode, childName);
        if (target.IsDirectory)
        {
            return false;
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, parent, childName))
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

        string childName = name.ToString();
        if (!Ext2DirectoryHelper.TryFind(_superblock, parent, name, out Ext2DirEntry entry))
        {
            return false;
        }

        Ext2Inode target = _superblock.ReadInode(entry.Inode, childName);
        if (!target.IsDirectory)
        {
            return false;
        }

        // Only "." and ".." may remain.
        List<Ext2DirEntry> entries = Ext2DirectoryHelper.ParseDirectory(_superblock, target);
        for (int i = 0; i < entries.Count; i++)
        {
            string entryName = entries[i].Name;
            if (entryName != "." && entryName != "..")
            {
                return false;
            }
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, parent, childName))
        {
            return false;
        }

        uint block = target.Block[0];
        if (block != 0)
        {
            _superblock.FreeBlock(block);
        }

        target.LinksCount = 0;
        _superblock.FreeInode(target.InodeNumber);

        // Removing a subdirectory drops one parent link.
        parent.LinksCount--;
        _superblock.WriteInode(parent);

        uint group = _superblock.GroupOfInode(target.InodeNumber);
        Ext2GroupDesc descriptor = _superblock.GetGroup(group);
        if (descriptor.UsedDirsCount > 0)
        {
            descriptor.UsedDirsCount--;
        }

        WriteUsedDirsCount(group);
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
        if (oldParent is not Ext2Inode oldDir || newParent is not Ext2Inode newDir)
        {
            return false;
        }

        string oldChildName = oldName.ToString();
        string newChildName = newName.ToString();

        if (!Ext2DirectoryHelper.TryFind(_superblock, oldDir, oldName, out Ext2DirEntry oldEntry))
        {
            return false;
        }

        if (Ext2DirectoryHelper.TryFind(_superblock, newDir, newName, out _))
        {
            // Replace semantics are not implemented: refuse an existing
            // destination instead of writing a duplicate name.
            return false;
        }

        byte fileType = oldEntry.FileType;
        if (fileType == Ext2InodeLayout.FileTypeUnknown)
        {
            Ext2Inode node = _superblock.ReadInode(oldEntry.Inode, oldChildName);
            fileType = Ext2DirectoryHelper.FileTypeFromMode(node.Mode);
        }

        if (!Ext2DirectoryHelper.AddEntry(_superblock, newDir, newChildName, oldEntry.Inode, fileType))
        {
            return false;
        }

        if (!Ext2DirectoryHelper.RemoveEntry(_superblock, oldDir, oldChildName))
        {
            Ext2DirectoryHelper.RemoveEntry(_superblock, newDir, newChildName);
            return false;
        }

        // A directory moved across parents keeps a ".." pointing at the
        // old parent; rewrite it.
        if (fileType == Ext2InodeLayout.FileTypeDir && !ReferenceEquals(oldDir, newDir))
        {
            Ext2Inode moved = _superblock.ReadInode(oldEntry.Inode, newChildName);
            uint block = moved.Block[0];
            if (block != 0)
            {
                byte[] dirBlock = new byte[_superblock.BlockSize];
                _superblock.ReadBlocks(block, 1, dirBlock);

                // ".." follows ".", whose record is the header plus one name
                // byte, padded to a 4-byte boundary.
                int dotDotPos = (Ext2InodeLayout.DirEntryNameOffset + 1 + 3) & ~3;
                BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryInodeOffset, 4), newDir.InodeNumber);
                _superblock.WriteBlocks(block, 1, dirBlock);
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

        stat.InodeNumber = node.InodeNumber;
        stat.Mode = ToVfsMode(node.Mode);
        stat.LinkCount = node.LinksCount;
        stat.Uid = node.Uid;
        stat.Gid = node.Gid;
        stat.DeviceId = 0;
        stat.Size = node.FullSize;
        stat.PreferredBlockSize = _superblock.BlockSize;
        stat.Blocks = node.Blocks;
        stat.AccessTime = new VfsTimespec(node.Atime, 0);
        stat.ModificationTime = new VfsTimespec(node.Mtime, 0);
        stat.ChangeTime = new VfsTimespec(node.Ctime, 0);
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
                node.Size = (uint)(newSize & 0xFFFF_FFFF);
                node.SizeHigh = (uint)(newSize >> 32);
                _superblock.WriteInode(node);
            }
        }

        if ((flags & SetAttrFlags.Mode) != 0)
        {
            // The file type is immutable; only permission bits change.
            ushort curType = (ushort)(node.Mode & Ext2InodeLayout.IFMT);
            ushort newPerms = (ushort)(ToExt2Mode(attributes.Mode) & ModeLowBits);
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

        if ((flags & SetAttrFlags.AccessTime) != 0)
        {
            node.Atime = (uint)attributes.AccessTime.Seconds;
        }

        if ((flags & SetAttrFlags.ModificationTime) != 0)
        {
            node.Mtime = (uint)attributes.ModificationTime.Seconds;
        }

        if ((flags & SetAttrFlags.ChangeTime) != 0)
        {
            node.Ctime = (uint)attributes.ChangeTime.Seconds;
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
    public bool TryReadLink(IVfsInode symlink, [NotNullWhen(true)] out string? target)
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
    /// Persist one group's used-directory count to its on-disk descriptor.
    /// The allocator writes the free counts only, so creating or removing a
    /// directory writes this field itself.
    /// </summary>
    /// <param name="group">Zero-based block group.</param>
    private void WriteUsedDirsCount(uint group)
    {
        // The descriptor table follows the superblock: block 2 with 1 KiB
        // blocks, block 1 otherwise.
        uint tableStart = _superblock.BlockSize == 1024 ? 2u : 1u;
        uint tableBytes = _superblock.GroupsCount * (uint)Ext2SuperblockLayout.GroupDescSize;
        uint tableBlocks = (tableBytes + _superblock.BlockSize - 1) / _superblock.BlockSize;
        byte[] table = new byte[tableBlocks * _superblock.BlockSize];
        _superblock.ReadBlocks(tableStart, tableBlocks, table);
        int offset = (int)group * Ext2SuperblockLayout.GroupDescSize;
        BitConverter.TryWriteBytes(table.AsSpan(offset + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), _superblock.GetGroup(group).UsedDirsCount);
        _superblock.WriteBlocks(tableStart, tableBlocks, table);
    }

    /// <summary>
    /// Convert a VFS mode to raw i_mode bits (file type plus the low 12 permission bits).
    /// </summary>
    /// <param name="mode">VFS mode.</param>
    private static ushort ToExt2Mode(VfsMode mode)
    {
        ushort type = (mode & VfsMode.FileTypeMask) switch
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
        return (ushort)(type | ((uint)mode & ModeLowBits));
    }

    /// <summary>
    /// Convert raw i_mode bits to a VFS mode.
    /// </summary>
    /// <param name="ext2Mode">Raw i_mode value.</param>
    private static VfsMode ToVfsMode(ushort ext2Mode)
    {
        ushort type = (ushort)(ext2Mode & Ext2InodeLayout.IFMT);
        VfsMode fileType = type switch
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

        return fileType | (VfsMode)(ext2Mode & ModeLowBits);
    }
}
