// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// Parsed ext2 directory entry. <see cref="Offset"/> is the byte position of
/// the entry within the buffer that produced it.
/// </summary>
internal sealed class Ext2DirEntry
{
    /// <summary>Inode number of the entry; 0 marks a free slot.</summary>
    public uint Inode { get; }

    /// <summary>Record length: bytes from this entry to the next one.</summary>
    public ushort RecLen { get; set; }

    /// <summary>Name length in bytes.</summary>
    public byte NameLen { get; }

    /// <summary>File type (one of the <c>FileType*</c> values).</summary>
    public byte FileType { get; }

    /// <summary>Decoded entry name.</summary>
    public string Name { get; }

    /// <summary>Byte offset of the entry within the parsed directory data.</summary>
    public int Offset { get; }

    /// <summary>
    /// Creates a parsed directory entry.
    /// </summary>
    /// <param name="inode">Inode number of the entry.</param>
    /// <param name="recLen">Record length in bytes.</param>
    /// <param name="nameLen">Name length in bytes.</param>
    /// <param name="fileType">File type value.</param>
    /// <param name="name">Decoded entry name.</param>
    /// <param name="offset">Byte offset within the parsed directory data.</param>
    public Ext2DirEntry(uint inode, ushort recLen, byte nameLen, byte fileType, string name, int offset)
    {
        Inode = inode;
        RecLen = recLen;
        NameLen = nameLen;
        FileType = fileType;
        Name = name;
        Offset = offset;
    }
}

/// <summary>
/// Parser and mutator for ext2 directory blocks. Operates over raw byte
/// spans read through the superblock; no I/O is performed here beyond the
/// block reads the superblock exposes.
/// </summary>
internal static class Ext2DirectoryHelper
{
    /// <summary>Upper bound on a directory's parsed size: larger sizes come from corrupt inodes.</summary>
    private const ulong MaxDirectorySize = 16 * 1024 * 1024;

    /// <summary>Upper bound on the blocks a single directory spans.</summary>
    private const uint MaxDirectoryBlocks = 4096;

    /// <summary>Upper bound on entries walked per parse: corrupt rec_len chains must terminate.</summary>
    private const int MaxParseIters = 8192;

    /// <summary>
    /// Parse the directory's entries. Stops on the first malformed record
    /// (zero or undersized rec_len, a name longer than its record, or a
    /// record leaving the data) instead of walking into the next block.
    /// </summary>
    /// <param name="sb">The mounted volume.</param>
    /// <param name="dir">Directory inode to list.</param>
    public static List<Ext2DirEntry> ParseDirectory(Ext2Superblock sb, Ext2Inode dir)
    {
        List<Ext2DirEntry> result = [];
        if (!dir.IsDirectory)
        {
            return result;
        }

        ulong size = dir.FullSize;
        if (size == 0 || size > MaxDirectorySize)
        {
            return result;
        }

        uint blocks = (uint)((size + sb.BlockSize - 1) / sb.BlockSize);
        if (blocks == 0 || blocks > MaxDirectoryBlocks)
        {
            return result;
        }

        byte[] data = new byte[blocks * sb.BlockSize];
        uint offset = 0;
        for (uint i = 0; i < blocks; i++)
        {
            uint blk = sb.GetBlockPointer(dir, i, false, out _);
            if (blk == 0)
            {
                break;
            }

            Span<byte> slice = data.AsSpan((int)offset, (int)sb.BlockSize);
            sb.ReadBlocks(blk, 1, slice);
            offset += sb.BlockSize;
        }

        int pos = 0;
        int iters = 0;
        while (pos + Ext2InodeLayout.DirEntryMinRecLen <= data.Length && (ulong)pos < size && iters < MaxParseIters)
        {
            iters++;
            if (pos + 8 > data.Length)
            {
                break;
            }

            uint inode = BitConverter.ToUInt32(data.AsSpan(pos + Ext2InodeLayout.DirEntryInodeOffset, 4));
            ushort recLen = BitConverter.ToUInt16(data.AsSpan(pos + Ext2InodeLayout.DirEntryRecLenOffset, 2));
            byte nameLen = data[pos + Ext2InodeLayout.DirEntryNameLenOffset];
            byte fileType = data[pos + Ext2InodeLayout.DirEntryFileTypeOffset];

            if (recLen == 0 || recLen < Ext2InodeLayout.DirEntryMinRecLen || pos + recLen > data.Length)
            {
                break;
            }

            if (nameLen > recLen - Ext2InodeLayout.DirEntryNameOffset)
            {
                break;
            }

            if (inode != 0)
            {
                int nameLenClamped = Math.Min((int)nameLen, recLen - Ext2InodeLayout.DirEntryNameOffset);
                string name;
                try
                {
                    name = global::System.Text.Encoding.UTF8.GetString(data, pos + Ext2InodeLayout.DirEntryNameOffset, nameLenClamped);
                }
                catch
                {
                    break;
                }

                result.Add(new Ext2DirEntry(inode, recLen, nameLen, fileType, name, pos));
            }

            pos += recLen;
        }

        return result;
    }

    /// <summary>
    /// Find a child entry by exact (case-sensitive) name.
    /// </summary>
    /// <param name="sb">The mounted volume.</param>
    /// <param name="dir">Directory to search.</param>
    /// <param name="name">Child name, a single path component.</param>
    /// <param name="entry">Matching entry on success.</param>
    /// <returns>true when the name exists in <paramref name="dir"/>.</returns>
    public static bool TryFind(Ext2Superblock sb, Ext2Inode dir, ReadOnlySpan<char> name, out Ext2DirEntry entry)
    {
        entry = null!;
        string target = name.ToString();
        List<Ext2DirEntry> entries = ParseDirectory(sb, dir);
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Name == target)
            {
                entry = entries[i];
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Insert an entry into a directory: splits the first entry with enough
    /// slack, reuses a free slot, or appends a fresh block. The directory
    /// size grows by whole blocks.
    /// </summary>
    /// <param name="sb">The mounted volume.</param>
    /// <param name="dir">Directory receiving the entry.</param>
    /// <param name="name">New entry name (UTF-8, at most 255 bytes).</param>
    /// <param name="inodeNumber">Inode number the entry points at.</param>
    /// <param name="fileType">File type value recorded in the entry.</param>
    /// <returns>true when the entry was inserted.</returns>
    public static bool AddEntry(Ext2Superblock sb, Ext2Inode dir, string name, uint inodeNumber, byte fileType)
    {
        if (name.Length > 255)
        {
            return false;
        }

        byte[] nameBytes = global::System.Text.Encoding.UTF8.GetBytes(name);
        if (nameBytes.Length > 255)
        {
            return false;
        }

        int needed = Ext2InodeLayout.DirEntryNameOffset + nameBytes.Length;
        needed = (needed + 3) & ~3;

        uint blocks = (uint)((dir.FullSize + sb.BlockSize - 1) / sb.BlockSize);
        if (blocks == 0)
        {
            // Need at least one block for directory.
            if (!AllocateDirBlock(sb, dir))
            {
                return false;
            }

            blocks = 1;
        }

        byte[] data = new byte[blocks * sb.BlockSize];
        for (uint i = 0; i < blocks; i++)
        {
            uint blk = sb.GetBlockPointer(dir, i, false, out _);
            if (blk == 0)
            {
                break;
            }

            sb.ReadBlocks(blk, 1, data.AsSpan((int)i * (int)sb.BlockSize, (int)sb.BlockSize));
        }

        // Search for free space.
        int pos = 0;
        while (pos < data.Length)
        {
            uint inode = BitConverter.ToUInt32(data.AsSpan(pos + Ext2InodeLayout.DirEntryInodeOffset, 4));
            ushort recLen = BitConverter.ToUInt16(data.AsSpan(pos + Ext2InodeLayout.DirEntryRecLenOffset, 2));
            if (recLen == 0)
            {
                break;
            }

            int idealLen = 0;
            if (inode != 0)
            {
                byte nl = data[pos + Ext2InodeLayout.DirEntryNameLenOffset];
                idealLen = (Ext2InodeLayout.DirEntryNameOffset + nl + 3) & ~3;
            }
            else
            {
                idealLen = 0;
            }

            int remaining = recLen - idealLen;
            if (inode != 0 && remaining >= needed)
            {
                // Split.
                ushort newRecLen = (ushort)idealLen;
                BitConverter.TryWriteBytes(data.AsSpan(pos + Ext2InodeLayout.DirEntryRecLenOffset, 2), newRecLen);
                int newPos = pos + idealLen;
                // Write new entry.
                BitConverter.TryWriteBytes(data.AsSpan(newPos + Ext2InodeLayout.DirEntryInodeOffset, 4), inodeNumber);
                BitConverter.TryWriteBytes(data.AsSpan(newPos + Ext2InodeLayout.DirEntryRecLenOffset, 2), (ushort)(recLen - idealLen));
                data[newPos + Ext2InodeLayout.DirEntryNameLenOffset] = (byte)nameBytes.Length;
                data[newPos + Ext2InodeLayout.DirEntryFileTypeOffset] = fileType;
                nameBytes.CopyTo(data.AsSpan(newPos + Ext2InodeLayout.DirEntryNameOffset, nameBytes.Length));
                // Zero pad?
                for (int z = nameBytes.Length; z < (recLen - idealLen) - Ext2InodeLayout.DirEntryNameOffset; z++)
                {
                    data[newPos + Ext2InodeLayout.DirEntryNameOffset + z] = 0;
                }

                // Write back.
                for (uint i = 0; i < blocks; i++)
                {
                    uint blk = sb.GetBlockPointer(dir, i, false, out _);
                    sb.WriteBlocks(blk, 1, data.AsSpan((int)i * (int)sb.BlockSize, (int)sb.BlockSize));
                }

                return true;
            }

            if (inode == 0 && recLen >= needed)
            {
                // Reuse empty entry (may need to keep recLen as is if at end).
                BitConverter.TryWriteBytes(data.AsSpan(pos + Ext2InodeLayout.DirEntryInodeOffset, 4), inodeNumber);
                // Keep recLen
                data[pos + Ext2InodeLayout.DirEntryNameLenOffset] = (byte)nameBytes.Length;
                data[pos + Ext2InodeLayout.DirEntryFileTypeOffset] = fileType;
                nameBytes.CopyTo(data.AsSpan(pos + Ext2InodeLayout.DirEntryNameOffset, nameBytes.Length));
                for (int z = nameBytes.Length; z < recLen - Ext2InodeLayout.DirEntryNameOffset; z++)
                {
                    data[pos + Ext2InodeLayout.DirEntryNameOffset + z] = 0;
                }

                for (uint i = 0; i < blocks; i++)
                {
                    uint blk = sb.GetBlockPointer(dir, i, false, out _);
                    sb.WriteBlocks(blk, 1, data.AsSpan((int)i * (int)sb.BlockSize, (int)sb.BlockSize));
                }

                return true;
            }

            pos += recLen;
        }

        // No space, need new block.
        if (!AllocateDirBlock(sb, dir))
        {
            return false;
        }

        // Retry by adding at new block's start.
        uint newBlockIdx = blocks; // 0-based
        byte[] blockData = new byte[sb.BlockSize];
        BitConverter.TryWriteBytes(blockData.AsSpan(Ext2InodeLayout.DirEntryInodeOffset, 4), inodeNumber);
        BitConverter.TryWriteBytes(blockData.AsSpan(Ext2InodeLayout.DirEntryRecLenOffset, 2), (ushort)sb.BlockSize);
        blockData[Ext2InodeLayout.DirEntryNameLenOffset] = (byte)nameBytes.Length;
        blockData[Ext2InodeLayout.DirEntryFileTypeOffset] = fileType;
        nameBytes.CopyTo(blockData.AsSpan(Ext2InodeLayout.DirEntryNameOffset, nameBytes.Length));
        uint nb = sb.GetBlockPointer(dir, newBlockIdx, false, out _);
        sb.WriteBlocks(nb, 1, blockData);
        // Update dir size.
        dir.Size = (uint)((newBlockIdx + 1) * sb.BlockSize);
        sb.WriteInode(dir);
        return true;
    }

    /// <summary>
    /// Append one block to a directory and grow its size.
    /// </summary>
    /// <param name="sb">The mounted volume.</param>
    /// <param name="dir">Directory to grow.</param>
    /// <returns>true when a block was allocated and the size updated.</returns>
    private static bool AllocateDirBlock(Ext2Superblock sb, Ext2Inode dir)
    {
        uint idx = (uint)((dir.FullSize + sb.BlockSize - 1) / sb.BlockSize);
        uint blk = sb.GetBlockPointer(dir, idx, true, out _);
        if (blk == 0)
        {
            return false;
        }

        dir.Size = (uint)((idx + 1) * sb.BlockSize);
        sb.WriteInode(dir);
        // Initialize new block with empty dir? Actually should be zeroed, then first entry will be written by caller.
        // Ensure previous block's last entry extends to block boundary.
        // We do that by adjusting last entry's rec_len to remaining space.
        if (idx > 0)
        {
            // Reload previous block to adjust? The AddEntry path that allocated will handle new block itself.
        }

        return true;
    }

    /// <summary>
    /// Remove an entry by merging its record into the previous one (or
    /// zeroing the inode when it is the first entry).
    /// </summary>
    /// <param name="sb">The mounted volume.</param>
    /// <param name="dir">Directory holding the entry.</param>
    /// <param name="name">Name of the entry to remove.</param>
    /// <returns>true when the entry was found and removed.</returns>
    public static bool RemoveEntry(Ext2Superblock sb, Ext2Inode dir, string name)
    {
        if (dir.FullSize == 0)
        {
            return false;
        }

        uint blocks = (uint)((dir.FullSize + sb.BlockSize - 1) / sb.BlockSize);
        byte[] data = new byte[blocks * sb.BlockSize];
        for (uint i = 0; i < blocks; i++)
        {
            uint blk = sb.GetBlockPointer(dir, i, false, out _);
            if (blk == 0)
            {
                break;
            }

            sb.ReadBlocks(blk, 1, data.AsSpan((int)i * (int)sb.BlockSize, (int)sb.BlockSize));
        }

        int pos = 0;
        int prevPos = -1;
        ushort prevRecLen = 0;
        while (pos < data.Length)
        {
            uint inode = BitConverter.ToUInt32(data.AsSpan(pos + Ext2InodeLayout.DirEntryInodeOffset, 4));
            ushort recLen = BitConverter.ToUInt16(data.AsSpan(pos + Ext2InodeLayout.DirEntryRecLenOffset, 2));
            if (recLen == 0)
            {
                break;
            }

            byte nameLen = data[pos + Ext2InodeLayout.DirEntryNameLenOffset];
            if (inode != 0)
            {
                int cl = Math.Min(nameLen, recLen - Ext2InodeLayout.DirEntryNameOffset);
                string n = global::System.Text.Encoding.UTF8.GetString(data, pos + Ext2InodeLayout.DirEntryNameOffset, cl);
                if (n == name)
                {
                    // Found. Mark inode 0 and coalesce with previous.
                    if (prevPos >= 0)
                    {
                        // Extend previous rec_len
                        ushort newLen = (ushort)(prevRecLen + recLen);
                        BitConverter.TryWriteBytes(data.AsSpan(prevPos + Ext2InodeLayout.DirEntryRecLenOffset, 2), newLen);
                    }
                    else
                    {
                        // First entry: just zero inode
                        BitConverter.TryWriteBytes(data.AsSpan(pos + Ext2InodeLayout.DirEntryInodeOffset, 4), (uint)0);
                    }

                    // If we coalesced, we need to write back all.
                    // If prevPos >=0, we effectively removed entry by merging; otherwise zeroed inode keeps rec_len.
                    for (uint i = 0; i < blocks; i++)
                    {
                        uint blk = sb.GetBlockPointer(dir, i, false, out _);
                        sb.WriteBlocks(blk, 1, data.AsSpan((int)i * (int)sb.BlockSize, (int)sb.BlockSize));
                    }

                    // If entry was coalesced, the hole is merged; if first, inode 0 stays with same rec_len and will be reused.
                    // For simplicity, when coalesced we already extended prev; need to handle case where first entry and there are more? Zeroing is fine.
                    if (prevPos >= 0)
                    {
                        // We already extended prev, but current entry's bytes remain; need to zero? Not needed, rec_len covers.
                    }

                    return true;
                }
            }

            prevPos = pos;
            prevRecLen = recLen;
            pos += recLen;
        }

        return false;
    }

    /// <summary>
    /// Map an inode mode's file-type nibble to a directory entry file type.
    /// </summary>
    /// <param name="mode">Raw i_mode value.</param>
    public static byte FileTypeFromMode(ushort mode)
    {
        ushort type = (ushort)(mode & Ext2InodeLayout.IFMT);
        return type switch
        {
            Ext2InodeLayout.IFREG => Ext2InodeLayout.FileTypeRegFile,
            Ext2InodeLayout.IFDIR => Ext2InodeLayout.FileTypeDir,
            Ext2InodeLayout.IFLNK => Ext2InodeLayout.FileTypeSymlink,
            Ext2InodeLayout.IFCHR => Ext2InodeLayout.FileTypeChrDev,
            Ext2InodeLayout.IFBLK => Ext2InodeLayout.FileTypeBlkDev,
            Ext2InodeLayout.IFIFO => Ext2InodeLayout.FileTypeFifo,
            Ext2InodeLayout.IFSOCK => Ext2InodeLayout.FileTypeSock,
            _ => Ext2InodeLayout.FileTypeUnknown,
        };
    }
}
