// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// Parameters for <see cref="Ext2FilesystemType.TryFormat(global::System.ReadOnlySpan{char}, IVfsFormatOptions)"/>.
/// </summary>
public sealed class Ext2FormatOptions : IVfsFormatOptions
{
    /// <summary>Block size in bytes (1024, 2048, or 4096).</summary>
    public uint BlockSize { get; set; } = 1024;

    /// <summary>Volume label, up to 16 characters.</summary>
    public string VolumeLabel { get; set; } = "";
}

/// <summary>
/// Lays down a fresh ext2 volume on an <see cref="IBlockDevice"/>: writes
/// the superblock, group descriptors with bitmaps and inode tables, and a
/// root directory holding "." and "..". Pure I/O against the device — no
/// superblock or VFS state.
/// </summary>
internal static class Ext2Formatter
{
    /// <summary>Smallest volume (in filesystem blocks) <see cref="Format"/> will attempt.</summary>
    private const uint MinTotalBlocks = 16;

    /// <summary>Blocks per group for 1024-byte volumes (8 MiB groups).</summary>
    private const uint BlocksPerGroup1K = 8192u;

    /// <summary>Blocks per group for 2048-byte volumes (8 MiB groups).</summary>
    private const uint BlocksPerGroup2K = 4096u;

    /// <summary>Blocks per group for 4096-byte volumes (8 MiB groups).</summary>
    private const uint BlocksPerGroup4K = 2048u;

    /// <summary>Inodes per group, derived as one quarter of the blocks per group.</summary>
    private const uint InodesPerBlockRatio = 4;

    /// <summary>Smallest inode count per group; keeps tiny volumes usable.</summary>
    private const uint MinInodesPerGroup = 16;

    /// <summary>Inodes 1 through 10 are reserved; the root takes inode 2.</summary>
    private const uint ReservedInodeCount = 10;

    /// <summary>Inodes consumed at format time (the 10 reserved plus the root).</summary>
    private const uint UsedInodesAtFormat = 11;

    /// <summary>Filesystem state stamped at format time: cleanly unmounted.</summary>
    private const ushort ValidState = 1;

    /// <summary>Error behavior stamped at format time: continue on errors.</summary>
    private const ushort ErrorsContinue = 1;

    /// <summary>Creator OS stamped at format time: Linux.</summary>
    private const uint CreatorLinux = 0;

    /// <summary>Inode size stamped at format time (128 bytes).</summary>
    private const int Rev0InodeSize = 128;

    /// <summary>Incompatible feature flag stamped at format time: directory entries carry a file type.</summary>
    private const uint FeatureIncompatFiletype = 2;

    /// <summary>Longest volume label in characters.</summary>
    private const int MaxVolumeLabelLength = 16;

    /// <summary>
    /// Format <paramref name="device"/> with a fresh ext2 volume.
    /// </summary>
    /// <param name="device">Block device to format.</param>
    /// <param name="options">Block size and label; null selects defaults.</param>
    /// <returns>true when the volume was written.</returns>
    public static bool Format(IBlockDevice device, Ext2FormatOptions? options)
    {
        uint blockSize = options?.BlockSize ?? 1024;
        if (blockSize != 1024 && blockSize != 2048 && blockSize != 4096)
        {
            return false;
        }

        uint logBlockSize = 0;
        if (blockSize == 2048)
        {
            logBlockSize = 1;
        }
        else if (blockSize == 4096)
        {
            logBlockSize = 2;
        }

        ulong devBlockSize = device.BlockSize;
        if (blockSize % devBlockSize != 0)
        {
            return false;
        }

        ulong totalBytes = device.BlockCount * devBlockSize;
        uint totalBlocks = (uint)(totalBytes / blockSize);
        if (totalBlocks < MinTotalBlocks)
        {
            return false;
        }

        // Groups stay near 8 MiB regardless of block size; tiny devices
        // collapse to a single group.
        uint blocksPerGroup = blockSize == 1024 ? BlocksPerGroup1K : (blockSize == 2048 ? BlocksPerGroup2K : BlocksPerGroup4K);
        if (blocksPerGroup > totalBlocks)
        {
            blocksPerGroup = totalBlocks;
        }

        uint inodesPerGroup = blocksPerGroup / InodesPerBlockRatio;
        if (inodesPerGroup < MinInodesPerGroup)
        {
            inodesPerGroup = MinInodesPerGroup;
        }

        uint groups = (totalBlocks + blocksPerGroup - 1) / blocksPerGroup;
        uint totalInodes = inodesPerGroup * groups;
        uint freeInodes = totalInodes - UsedInodesAtFormat;
        uint overheadBlocks = 0;
        uint gdBlocks = (groups * (uint)Ext2SuperblockLayout.GroupDescSize + blockSize - 1) / blockSize;
        uint inodeTableBlocksPerGroup = (inodesPerGroup * 128 + blockSize - 1) / blockSize;
        for (uint g = 0; g < groups; g++)
        {
            uint blocksInGroup = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint overhead = 2 + inodeTableBlocksPerGroup;
            if (g == 0)
            {
                overhead += 1;
                overhead += gdBlocks;
                if (blockSize == 1024)
                {
                    overhead += 1;
                }
            }

            if (overhead > blocksInGroup)
            {
                return false;
            }

            overheadBlocks += overhead;
        }

        uint freeBlocks = totalBlocks - overheadBlocks - 1;
        if (freeBlocks == 0 || freeInodes == 0)
        {
            return false;
        }
        byte[] sb = new byte[Ext2SuperblockLayout.SuperblockSize];
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.InodesCountOffset, 4), totalInodes);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.BlocksCountOffset, 4), totalBlocks);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.RBlocksCountOffset, 4), (uint)0);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FreeBlocksCountOffset, 4), freeBlocks);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FreeInodesCountOffset, 4), freeInodes);
        uint firstDataBlock = blockSize == 1024 ? 1u : 0u;
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FirstDataBlockOffset, 4), firstDataBlock);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.LogBlockSizeOffset, 4), logBlockSize);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.LogFragSizeOffset, 4), logBlockSize);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.BlocksPerGroupOffset, 4), blocksPerGroup);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FragsPerGroupOffset, 4), blocksPerGroup);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.InodesPerGroupOffset, 4), inodesPerGroup);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.MtimeOffset, 4), now);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.WtimeOffset, 4), now);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.MagicOffset, 2), Ext2SuperblockLayout.Magic);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.StateOffset, 2), (ushort)ValidState);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.ErrorsOffset, 2), (ushort)ErrorsContinue);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.CreatorOsOffset, 4), (uint)CreatorLinux);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.RevLevelOffset, 4), Ext2SuperblockLayout.RevDynamic);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FirstInoOffset, 4), Ext2SuperblockLayout.DefaultFirstIno);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.InodeSizeOffset, 2), (ushort)Rev0InodeSize);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FeatureCompatOffset, 4), (uint)0);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FeatureIncompatOffset, 4), (uint)FeatureIncompatFiletype);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FeatureRoCompatOffset, 4), (uint)0);
        string vol = options?.VolumeLabel ?? "";
        if (vol.Length > MaxVolumeLabelLength)
        {
            vol = vol.Substring(0, MaxVolumeLabelLength);
        }

        global::System.Text.Encoding.ASCII.GetBytes(vol).CopyTo(sb.AsSpan(Ext2SuperblockLayout.VolumeNameOffset, vol.Length));

        WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset, sb);

        uint gdStartBlock = blockSize == 1024 ? 2u : 1u;
        byte[] gdBuf = new byte[gdBlocks * blockSize];

        // Group 0's bitmaps sit after the descriptor table (the superblock
        // and descriptors occupy its first blocks); other groups keep
        // bitmaps and table at the group's head. No backup superblocks are
        // written. Free counts are filled in after the bitmaps below.
        List<uint> blockBitmapBlocks = new();
        List<uint> inodeBitmapBlocks = new();
        List<uint> inodeTableStarts = new();

        for (uint g = 0; g < groups; g++)
        {
            uint groupStart = g * blocksPerGroup;
            uint groupEnd = groupStart + (g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup);
            uint bBitmap, iBitmap, iTable;
            if (g == 0)
            {
                bBitmap = gdStartBlock + gdBlocks;
                iBitmap = bBitmap + 1;
                iTable = iBitmap + 1;
            }
            else
            {
                bBitmap = groupStart;
                iBitmap = groupStart + 1;
                iTable = groupStart + 2;
            }

            if (bBitmap >= groupEnd || iBitmap >= groupEnd || iTable + inodeTableBlocksPerGroup > groupEnd)
            {
                return false;
            }

            blockBitmapBlocks.Add(bBitmap);
            inodeBitmapBlocks.Add(iBitmap);
            inodeTableStarts.Add(iTable);

            int off = (int)g * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4), bBitmap);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4), iBitmap);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4), iTable);
        }

        WriteBlocks(device, gdStartBlock, gdBlocks, blockSize, gdBuf);

        for (uint g = 0; g < groups; g++)
        {
            uint groupStart = g * blocksPerGroup;
            uint groupBlocks = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint bBitmap = blockBitmapBlocks[(int)g];
            uint iBitmap = inodeBitmapBlocks[(int)g];
            uint iTable = inodeTableStarts[(int)g];

            // Metadata blocks (superblock, descriptors, bitmaps, tables,
            // root directory) are marked allocated; data blocks stay free.
            byte[] bBmp = new byte[blockSize];
            for (uint b = 0; b < groupBlocks; b++)
            {
                uint absBlock = groupStart + b;
                bool isAllocated = false;
                if (g == 0)
                {
                    if (blockSize == 1024)
                    {
                        if (absBlock == 0)
                        {
                            isAllocated = true;
                        }

                        if (absBlock == 1)
                        {
                            isAllocated = true;
                        }
                    }
                    else
                    {
                        if (absBlock == 0)
                        {
                            isAllocated = true;
                        }
                    }

                    if (absBlock >= gdStartBlock && absBlock < gdStartBlock + gdBlocks)
                    {
                        isAllocated = true;
                    }

                    if (absBlock == bBitmap || absBlock == iBitmap)
                    {
                        isAllocated = true;
                    }

                    if (absBlock >= iTable && absBlock < iTable + inodeTableBlocksPerGroup)
                    {
                        isAllocated = true;
                    }

                    if (g == 0 && absBlock == iTable + inodeTableBlocksPerGroup)
                    {
                        isAllocated = true;
                    }
                }
                else
                {
                    if (b == 0)
                    {
                        isAllocated = true; // block bitmap itself
                    }

                    if (b == 1)
                    {
                        isAllocated = true; // inode bitmap
                    }

                    if (b >= 2 && b < 2 + inodeTableBlocksPerGroup)
                    {
                        isAllocated = true;
                    }
                }

                if (isAllocated)
                {
                    bBmp[b / 8] |= (byte)(1 << (int)(b % 8));
                }

                if (absBlock >= totalBlocks)
                {
                    bBmp[b / 8] |= (byte)(1 << (int)(b % 8));
                }
            }

            WriteBlocks(device, bBitmap, 1, blockSize, bBmp);

            // Reserved inodes 1..10 (including the root at 2) start allocated.
            byte[] iBmp = new byte[blockSize];
            uint inodesInThisGroup = g == groups - 1 ? totalInodes - g * inodesPerGroup : inodesPerGroup;
            for (uint i = 0; i < inodesInThisGroup; i++)
            {
                uint ino = g * inodesPerGroup + i + 1;
                if (ino <= ReservedInodeCount)
                {
                    iBmp[i / 8] |= (byte)(1 << (int)(i % 8));
                }
            }

            WriteBlocks(device, iBitmap, 1, blockSize, iBmp);

            byte[] zeroTable = new byte[inodeTableBlocksPerGroup * blockSize];
            WriteBlocks(device, iTable, inodeTableBlocksPerGroup, blockSize, zeroTable);
        }

        // The bitmaps are final: recount free blocks and inodes per group
        // and stamp the descriptors a second time.
        for (uint g = 0; g < groups; g++)
        {
            uint groupStart = g * blocksPerGroup;
            uint groupBlocks = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint bBitmap = blockBitmapBlocks[(int)g];
            byte[] bBmp = new byte[blockSize];
            ReadBlocks(device, bBitmap, 1, blockSize, bBmp);
            int groupFreeBlocks = 0;
            for (uint b = 0; b < groupBlocks; b++)
            {
                if ((bBmp[b / 8] & (1 << (int)(b % 8))) == 0)
                {
                    groupFreeBlocks++;
                }
            }

            uint iBitmap = inodeBitmapBlocks[(int)g];
            byte[] iBmp = new byte[blockSize];
            ReadBlocks(device, iBitmap, 1, blockSize, iBmp);
            uint inodesInThisGroup = g == groups - 1 ? totalInodes - g * inodesPerGroup : inodesPerGroup;
            int groupFreeInodes = 0;
            for (uint i = 0; i < inodesInThisGroup; i++)
            {
                if ((iBmp[i / 8] & (1 << (int)(i % 8))) == 0)
                {
                    groupFreeInodes++;
                }
            }

            ushort usedDirs = 0;
            if (g == 0)
            {
                usedDirs = 1;
            }

            int off = (int)g * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4), bBitmap);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4), iBitmap);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4), inodeTableStarts[(int)g]);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2), (ushort)groupFreeBlocks);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2), (ushort)groupFreeInodes);
            BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), usedDirs);
        }

        WriteBlocks(device, gdStartBlock, gdBlocks, blockSize, gdBuf);

        uint totalFreeBlocks = 0;
        uint totalFreeInodes = 0;
        for (uint g = 0; g < groups; g++)
        {
            int off = (int)g * Ext2SuperblockLayout.GroupDescSize;
            totalFreeBlocks += BitConverter.ToUInt16(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2));
            totalFreeInodes += BitConverter.ToUInt16(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2));
        }

        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FreeBlocksCountOffset, 4), totalFreeBlocks);
        BitConverter.TryWriteBytes(sb.AsSpan(Ext2SuperblockLayout.FreeInodesCountOffset, 4), totalFreeInodes);
        WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset, sb);

        // Inode 2 is the root: index 1 within group 0's table.
        uint rootIdx = 1;
        uint rootTable = inodeTableStarts[0];
        uint inodesPerBlock = blockSize / 128;
        uint blockOff = rootIdx / inodesPerBlock;
        uint offInBlock = (rootIdx % inodesPerBlock) * 128;
        byte[] inodeBlock = new byte[blockSize];
        ReadBlocks(device, rootTable + blockOff, 1, blockSize, inodeBlock);
        Span<byte> raw = inodeBlock.AsSpan((int)offInBlock, 128);
        const ushort RootMode = (ushort)(Ext2InodeLayout.IFDIR | 0x1FF);
        const ushort RootLinks = 2;
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.ModeOffset, 2), RootMode);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.UidOffset, 2), (ushort)0);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.SizeOffset, 4), blockSize);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.AtimeOffset, 4), now);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.CtimeOffset, 4), now);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.MtimeOffset, 4), now);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.GidOffset, 2), (ushort)0);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.LinksCountOffset, 2), RootLinks);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.BlocksOffset, 4), (uint)(blockSize / 512));
        uint rootBlock = inodeTableStarts[0] + inodeTableBlocksPerGroup;
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.BlockOffset, 4), rootBlock);
        WriteBlocks(device, rootTable + blockOff, 1, blockSize, inodeBlock);

        // The root directory holds "." (itself) and ".." (itself).
        byte[] dirBlock = new byte[blockSize];
        BitConverter.TryWriteBytes(dirBlock.AsSpan(Ext2InodeLayout.DirEntryInodeOffset, 4), (uint)2);
        int dotLen = (Ext2InodeLayout.DirEntryNameOffset + 1 + 3) & ~3;
        BitConverter.TryWriteBytes(dirBlock.AsSpan(Ext2InodeLayout.DirEntryRecLenOffset, 2), (ushort)dotLen);
        dirBlock[Ext2InodeLayout.DirEntryNameLenOffset] = 1;
        dirBlock[Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        dirBlock[Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        int dotDotPos = dotLen;
        BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryInodeOffset, 4), (uint)2);
        ushort dotDotLen = (ushort)(blockSize - dotLen);
        BitConverter.TryWriteBytes(dirBlock.AsSpan(dotDotPos + Ext2InodeLayout.DirEntryRecLenOffset, 2), dotDotLen);
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameLenOffset] = 2;
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        dirBlock[dotDotPos + Ext2InodeLayout.DirEntryNameOffset + 1] = (byte)'.';
        WriteBlocks(device, rootBlock, 1, blockSize, dirBlock);

        device.Flush();
        return true;
    }

    /// <summary>
    /// Wipe the filesystem signature so the volume no longer mounts. The
    /// underlying device is not zeroed in full.
    /// </summary>
    /// <param name="device">Block device holding the volume.</param>
    /// <returns>true when the signature was wiped.</returns>
    public static bool Destroy(IBlockDevice device)
    {
        try
        {
            WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset + Ext2SuperblockLayout.MagicOffset, new byte[2]);
            device.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write bytes at an arbitrary byte offset, reading back the touched
    /// device blocks when the range is unaligned.
    /// </summary>
    /// <param name="device">Block device to write to.</param>
    /// <param name="byteOffset">Byte offset of the first byte.</param>
    /// <param name="data">Bytes to write.</param>
    private static void WriteBytes(IBlockDevice device, int byteOffset, ReadOnlySpan<byte> data)
    {
        ulong devBlockSize = device.BlockSize;
        ulong lba = (ulong)byteOffset / devBlockSize;
        int offsetInBlock = (int)((ulong)byteOffset % devBlockSize);
        if (offsetInBlock == 0 && (ulong)data.Length % devBlockSize == 0)
        {
            ulong blocks = (ulong)data.Length / devBlockSize;
            device.WriteBlock(lba, blocks, data);
        }
        else
        {
            ulong start = lba;
            ulong endByte = (ulong)byteOffset + (ulong)data.Length;
            ulong endLba = (endByte + devBlockSize - 1) / devBlockSize;
            ulong count = endLba - start;
            byte[] tmp = new byte[count * devBlockSize];
            device.ReadBlock(start, count, tmp);
            data.CopyTo(tmp.AsSpan(offsetInBlock, data.Length));
            device.WriteBlock(start, count, tmp);
        }
    }

    /// <summary>
    /// Write <paramref name="count"/> filesystem blocks.
    /// </summary>
    /// <param name="device">Block device to write to.</param>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to write.</param>
    /// <param name="blockSize">Filesystem block size in bytes.</param>
    /// <param name="data">Source bytes.</param>
    private static void WriteBlocks(IBlockDevice device, uint ext2Block, uint count, uint blockSize, ReadOnlySpan<byte> data)
    {
        ulong devBlockSize = device.BlockSize;
        ulong byteOffset = (ulong)ext2Block * blockSize;
        ulong lba = byteOffset / devBlockSize;
        ulong devBlocks = (ulong)count * blockSize / devBlockSize;
        device.WriteBlock(lba, devBlocks, data);
    }

    /// <summary>
    /// Read <paramref name="count"/> filesystem blocks.
    /// </summary>
    /// <param name="device">Block device to read from.</param>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to read.</param>
    /// <param name="blockSize">Filesystem block size in bytes.</param>
    /// <param name="dest">Destination buffer.</param>
    private static void ReadBlocks(IBlockDevice device, uint ext2Block, uint count, uint blockSize, Span<byte> dest)
    {
        ulong devBlockSize = device.BlockSize;
        ulong byteOffset = (ulong)ext2Block * blockSize;
        ulong lba = byteOffset / devBlockSize;
        ulong devBlocks = (ulong)count * blockSize / devBlockSize;
        device.ReadBlock(lba, devBlocks, dest);
    }
}
