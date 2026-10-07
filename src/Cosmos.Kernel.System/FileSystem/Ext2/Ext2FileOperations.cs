// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;

namespace Cosmos.Kernel.System.FileSystem.Ext2;

/// <summary>
/// Open-file byte I/O for ext2 inodes. Reads resolve logical blocks through
/// the superblock and copy them into the caller's buffer; writes allocate
/// blocks on demand and persist the inode size. Position advancement is the
/// caller's responsibility (matches the Linux VFS convention).
/// </summary>
internal sealed class Ext2FileOperations : IFileOperations
{
    private readonly Ext2Superblock _superblock;

    /// <summary>
    /// Reusable block buffer for the read/write loops; avoids one
    /// allocation per block on large sequential transfers.
    /// </summary>
    private byte[]? _blockBuffer;

    /// <summary>
    /// Creates file operations bound to a mounted volume.
    /// </summary>
    /// <param name="superblock">The mounted volume.</param>
    public Ext2FileOperations(Ext2Superblock superblock) => _superblock = superblock;

    /// <summary>
    /// Read bytes at the open file's position. Unallocated blocks (holes)
    /// read as zeros; reads past end-of-file return 0.
    /// </summary>
    /// <param name="openFile">Open file to read from.</param>
    /// <param name="buffer">Destination for the bytes.</param>
    /// <returns>Bytes read; <c>0</c> indicates end-of-file.</returns>
    public long Read(IVfsOpenFile openFile, Span<byte> buffer)
    {
        if (openFile.Inode is not Ext2Inode inode)
        {
            return 0;
        }

        long position = openFile.Position;

        // Short targets live inline in i_block, not in data blocks.
        if (inode.IsSymlink)
        {
            string target = ReadSymlinkTarget(inode);
            byte[] bytes = Encoding.UTF8.GetBytes(target);
            if (position >= bytes.Length)
            {
                return 0;
            }

            long toCopy = Math.Min(buffer.Length, bytes.Length - position);
            bytes.AsSpan((int)position, (int)toCopy).CopyTo(buffer);
            return toCopy;
        }

        ulong size = inode.FullSize;
        if (position < 0 || (ulong)position >= size)
        {
            return 0;
        }

        long remaining = (long)(size - (ulong)position);
        long toRead = Math.Min(buffer.Length, remaining);
        if (toRead <= 0)
        {
            return 0;
        }

        long copied = 0;
        uint blockSize = _superblock.BlockSize;
        while (copied < toRead)
        {
            long fileOffset = position + copied;
            uint logicalBlock = (uint)(fileOffset / blockSize);
            uint blockOffset = (uint)(fileOffset % blockSize);
            uint physicalBlock = _superblock.GetBlockPointer(inode, logicalBlock, false, out _);
            long chunk = Math.Min(blockSize - blockOffset, toRead - copied);
            if (physicalBlock == 0)
            {
                // Hole reads as zeros.
                buffer.Slice((int)copied, (int)chunk).Clear();
                copied += chunk;
                continue;
            }

            if (_blockBuffer is null || _blockBuffer.Length != blockSize)
            {
                _blockBuffer = new byte[blockSize];
            }

            byte[] blockBuffer = _blockBuffer;
            _superblock.ReadBlocks(physicalBlock, 1, blockBuffer);
            blockBuffer.AsSpan((int)blockOffset, (int)chunk).CopyTo(buffer.Slice((int)copied, (int)chunk));
            copied += chunk;
        }

        return copied;
    }

    /// <summary>
    /// Write bytes at the open file's position, allocating blocks as needed
    /// and growing the inode size. Seeks past end-of-file leave holes that
    /// later reads report as zeros.
    /// </summary>
    /// <param name="openFile">Open file to write to.</param>
    /// <param name="buffer">Bytes to write.</param>
    /// <returns>Bytes written.</returns>
    public long Write(IVfsOpenFile openFile, ReadOnlySpan<byte> buffer)
    {
        if (openFile.Inode is not Ext2Inode inode)
        {
            return 0;
        }

        if (inode.IsSymlink)
        {
            return 0;
        }

        if (buffer.Length == 0)
        {
            return 0;
        }

        long position = openFile.Position;
        if (position < 0)
        {
            return 0;
        }

        long endPosition = position + buffer.Length;
        if (endPosition < 0)
        {
            return 0;
        }

        // Seeks past end-of-file leave holes: the write loop below
        // allocates the touched blocks, and untouched gaps read as zeros.
        long written = 0;
        uint blockSize = _superblock.BlockSize;
        while (written < buffer.Length)
        {
            long fileOffset = position + written;
            uint logicalBlock = (uint)(fileOffset / blockSize);
            uint blockOffset = (uint)(fileOffset % blockSize);
            uint physicalBlock = _superblock.GetBlockPointer(inode, logicalBlock, true, out _);
            if (physicalBlock == 0)
            {
                break;
            }

            if (_blockBuffer is null || _blockBuffer.Length != blockSize)
            {
                _blockBuffer = new byte[blockSize];
            }

            byte[] blockBuffer = _blockBuffer;
            if (blockOffset != 0 || buffer.Length - written < blockSize)
            {
                _superblock.ReadBlocks(physicalBlock, 1, blockBuffer);
            }

            long chunk = Math.Min(blockSize - blockOffset, buffer.Length - written);
            buffer.Slice((int)written, (int)chunk).CopyTo(blockBuffer.AsSpan((int)blockOffset, (int)chunk));
            _superblock.WriteBlocks(physicalBlock, 1, blockBuffer);
            written += chunk;
        }

        if (written == 0)
        {
            return 0;
        }

        long newEnd = position + written;
        if ((ulong)newEnd > inode.FullSize)
        {
            // GetBlockPointer already added each block it allocated to
            // i_blocks, so only the size and mtime change here.
            inode.Size = (uint)(newEnd & 0xFFFF_FFFF);
            inode.SizeHigh = (uint)(newEnd >> 32);
            inode.Mtime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        _superblock.WriteInode(inode);
        return written;
    }

    /// <summary>
    /// Compute and apply a new file position. Positions beyond end-of-file
    /// are valid; a later write zero-fills the gap.
    /// </summary>
    /// <param name="openFile">Open file whose position is moved.</param>
    /// <param name="offset">Byte offset relative to <paramref name="whence"/>.</param>
    /// <param name="whence">Origin the offset is applied to.</param>
    /// <param name="newPosition">Resulting absolute position; 0 on failure.</param>
    /// <returns>true on success; false when the resulting position would be negative.</returns>
    public bool Seek(IVfsOpenFile openFile, long offset, SeekWhence whence, out long newPosition)
    {
        if (openFile.Inode is not Ext2Inode inode)
        {
            newPosition = 0;
            return false;
        }

        long basePosition = whence switch
        {
            SeekWhence.Set => 0,
            SeekWhence.Cur => openFile.Position,
            SeekWhence.End => (long)inode.FullSize,
            _ => -1,
        };

        if (basePosition < 0)
        {
            newPosition = 0;
            return false;
        }

        long target = basePosition + offset;
        if (target < 0)
        {
            newPosition = 0;
            return false;
        }

        openFile.Position = target;
        newPosition = target;
        return true;
    }

    /// <summary>
    /// Flush the file's data and metadata to the backing store.
    /// </summary>
    /// <param name="openFile">Open file to synchronize.</param>
    /// <returns>true on success.</returns>
    public bool Fsync(IVfsOpenFile openFile)
    {
        if (openFile.Inode is Ext2Inode inode)
        {
            _superblock.WriteInode(inode);
        }

        _superblock.Flush();
        return true;
    }

    /// <summary>
    /// Release driver-side state when the open file is closed (flushes first).
    /// </summary>
    /// <param name="openFile">Open file being closed.</param>
    public void Release(IVfsOpenFile openFile) => Fsync(openFile);

    /// <summary>
    /// Read a symlink's target: inline from i_block for short targets, from
    /// data blocks for long ones.
    /// </summary>
    /// <param name="inode">Symlink inode.</param>
    internal static string ReadSymlinkTarget(Ext2Inode inode)
    {
        ulong size = inode.FullSize;
        if (size <= Ext2InodeLayout.InlineSymlinkMax)
        {
            // Short targets are packed into i_block itself (60 bytes).
            Span<byte> raw = stackalloc byte[Ext2InodeLayout.InlineSymlinkMax];
            for (int i = 0; i < Ext2InodeLayout.BlockCount; i++)
            {
                BitConverter.TryWriteBytes(raw.Slice(i * 4, 4), inode.Block[i]);
            }

            return Encoding.UTF8.GetString(raw[..(int)size]);
        }
        else
        {
            uint blockSize = inode.Superblock.BlockSize;
            byte[] targetBytes = new byte[size];
            uint logicalBlock = 0;
            int copied = 0;
            while ((ulong)copied < size)
            {
                uint physicalBlock = inode.Superblock.GetBlockPointer(inode, logicalBlock, false, out _);
                if (physicalBlock == 0)
                {
                    break;
                }

                byte[] blockBuffer = new byte[blockSize];
                inode.Superblock.ReadBlocks(physicalBlock, 1, blockBuffer);
                int toCopy = (int)Math.Min(blockSize, size - (ulong)copied);
                blockBuffer.AsSpan(0, toCopy).CopyTo(targetBytes.AsSpan(copied, toCopy));
                copied += toCopy;
                logicalBlock++;
            }

            return Encoding.UTF8.GetString(targetBytes);
        }
    }

    /// <summary>
    /// Pack a short symlink target into i_block (60 bytes, no data blocks).
    /// </summary>
    /// <param name="inode">Symlink inode to fill.</param>
    /// <param name="target">Link target; must fit in 60 bytes.</param>
    internal static void WriteInlineSymlink(Ext2Inode inode, string target)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(target);

        // The zeroed buffer covers every i_block slot, so the slots past the
        // target are cleared as the loop packs it.
        Span<byte> raw = stackalloc byte[Ext2InodeLayout.InlineSymlinkMax];
        bytes.AsSpan().CopyTo(raw);
        for (int i = 0; i < Ext2InodeLayout.BlockCount; i++)
        {
            inode.Block[i] = BitConverter.ToUInt32(raw.Slice(i * 4, 4));
        }

        inode.Size = (uint)bytes.Length;
        inode.SizeHigh = 0;
        inode.Blocks = 0;
    }
}
