// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Storage;

namespace Cosmos.Kernel.System.FileSystem.Ext2;

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
        if (blockSize is not (1024 or 2048 or 4096))
        {
            return false;
        }

        uint logBlockSize = blockSize switch
        {
            2048 => 1u,
            4096 => 2u,
            _ => 0u,
        };

        ulong deviceBlockSize = device.BlockSize;
        if (blockSize % deviceBlockSize != 0)
        {
            return false;
        }

        ulong totalBytes = device.BlockCount * deviceBlockSize;
        uint totalBlocks = (uint)(totalBytes / blockSize);
        if (totalBlocks < MinTotalBlocks)
        {
            return false;
        }

        // Groups stay near 8 MiB regardless of block size; tiny devices
        // collapse to a single group.
        uint blocksPerGroup = blockSize switch
        {
            1024 => BlocksPerGroup1K,
            2048 => BlocksPerGroup2K,
            _ => BlocksPerGroup4K,
        };
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
        uint descriptorBlocks = (groups * (uint)Ext2SuperblockLayout.GroupDescSize + blockSize - 1) / blockSize;
        uint inodeTableBlocksPerGroup = (inodesPerGroup * Rev0InodeSize + blockSize - 1) / blockSize;
        for (uint g = 0; g < groups; g++)
        {
            uint blocksInGroup = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint overhead = 2 + inodeTableBlocksPerGroup;
            if (g == 0)
            {
                overhead += 1;
                overhead += descriptorBlocks;
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

        byte[] superblock = new byte[Ext2SuperblockLayout.SuperblockSize];
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.InodesCountOffset, 4), totalInodes);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.BlocksCountOffset, 4), totalBlocks);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.RBlocksCountOffset, 4), (uint)0);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FreeBlocksCountOffset, 4), freeBlocks);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FreeInodesCountOffset, 4), freeInodes);
        uint firstDataBlock = blockSize == 1024 ? 1u : 0u;
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FirstDataBlockOffset, 4), firstDataBlock);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.LogBlockSizeOffset, 4), logBlockSize);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.LogFragSizeOffset, 4), logBlockSize);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.BlocksPerGroupOffset, 4), blocksPerGroup);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FragsPerGroupOffset, 4), blocksPerGroup);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.InodesPerGroupOffset, 4), inodesPerGroup);
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.MtimeOffset, 4), now);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.WtimeOffset, 4), now);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.MagicOffset, 2), Ext2SuperblockLayout.Magic);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.StateOffset, 2), ValidState);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.ErrorsOffset, 2), ErrorsContinue);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.CreatorOsOffset, 4), CreatorLinux);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.RevLevelOffset, 4), Ext2SuperblockLayout.RevDynamic);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FirstInoOffset, 4), Ext2SuperblockLayout.DefaultFirstIno);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.InodeSizeOffset, 2), (ushort)Rev0InodeSize);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FeatureCompatOffset, 4), (uint)0);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FeatureIncompatOffset, 4), FeatureIncompatFiletype);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FeatureRoCompatOffset, 4), (uint)0);
        string volumeLabel = options?.VolumeLabel ?? "";
        if (volumeLabel.Length > MaxVolumeLabelLength)
        {
            volumeLabel = volumeLabel.Substring(0, MaxVolumeLabelLength);
        }

        global::System.Text.Encoding.ASCII.GetBytes(volumeLabel).CopyTo(superblock.AsSpan(Ext2SuperblockLayout.VolumeNameOffset, volumeLabel.Length));

        WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset, superblock);

        uint descriptorStartBlock = blockSize == 1024 ? 2u : 1u;
        byte[] descriptorTable = new byte[descriptorBlocks * blockSize];

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
            uint blockBitmapBlock, inodeBitmapBlock, inodeTableStart;
            if (g == 0)
            {
                blockBitmapBlock = descriptorStartBlock + descriptorBlocks;
                inodeBitmapBlock = blockBitmapBlock + 1;
                inodeTableStart = inodeBitmapBlock + 1;
            }
            else
            {
                blockBitmapBlock = groupStart;
                inodeBitmapBlock = groupStart + 1;
                inodeTableStart = groupStart + 2;
            }

            if (blockBitmapBlock >= groupEnd || inodeBitmapBlock >= groupEnd || inodeTableStart + inodeTableBlocksPerGroup > groupEnd)
            {
                return false;
            }

            blockBitmapBlocks.Add(blockBitmapBlock);
            inodeBitmapBlocks.Add(inodeBitmapBlock);
            inodeTableStarts.Add(inodeTableStart);

            int descriptorOffset = (int)g * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4), blockBitmapBlock);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4), inodeBitmapBlock);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4), inodeTableStart);
        }

        WriteBlocks(device, descriptorStartBlock, descriptorBlocks, blockSize, descriptorTable);

        for (uint g = 0; g < groups; g++)
        {
            uint groupStart = g * blocksPerGroup;
            uint groupBlocks = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint blockBitmapBlock = blockBitmapBlocks[(int)g];
            uint inodeBitmapBlock = inodeBitmapBlocks[(int)g];
            uint inodeTableStart = inodeTableStarts[(int)g];

            // Metadata blocks (superblock, descriptors, bitmaps, tables,
            // root directory) are marked allocated; data blocks stay free.
            byte[] blockBitmap = new byte[blockSize];
            for (uint b = 0; b < groupBlocks; b++)
            {
                uint absoluteBlock = groupStart + b;
                bool isAllocated = false;
                if (g == 0)
                {
                    if (blockSize == 1024)
                    {
                        if (absoluteBlock == 0)
                        {
                            isAllocated = true;
                        }

                        if (absoluteBlock == 1)
                        {
                            isAllocated = true;
                        }
                    }
                    else
                    {
                        if (absoluteBlock == 0)
                        {
                            isAllocated = true;
                        }
                    }

                    if (absoluteBlock >= descriptorStartBlock && absoluteBlock < descriptorStartBlock + descriptorBlocks)
                    {
                        isAllocated = true;
                    }

                    if (absoluteBlock == blockBitmapBlock || absoluteBlock == inodeBitmapBlock)
                    {
                        isAllocated = true;
                    }

                    if (absoluteBlock >= inodeTableStart && absoluteBlock < inodeTableStart + inodeTableBlocksPerGroup)
                    {
                        isAllocated = true;
                    }

                    if (absoluteBlock == inodeTableStart + inodeTableBlocksPerGroup)
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
                    blockBitmap[b / 8] |= (byte)(1 << (int)(b % 8));
                }

                if (absoluteBlock >= totalBlocks)
                {
                    blockBitmap[b / 8] |= (byte)(1 << (int)(b % 8));
                }
            }

            WriteBlocks(device, blockBitmapBlock, 1, blockSize, blockBitmap);

            // Reserved inodes 1..10 (including the root at 2) start allocated.
            byte[] inodeBitmap = new byte[blockSize];
            uint inodesInThisGroup = g == groups - 1 ? totalInodes - g * inodesPerGroup : inodesPerGroup;
            for (uint i = 0; i < inodesInThisGroup; i++)
            {
                uint inodeNumber = g * inodesPerGroup + i + 1;
                if (inodeNumber <= ReservedInodeCount)
                {
                    inodeBitmap[i / 8] |= (byte)(1 << (int)(i % 8));
                }
            }

            WriteBlocks(device, inodeBitmapBlock, 1, blockSize, inodeBitmap);

            byte[] zeroTable = new byte[inodeTableBlocksPerGroup * blockSize];
            WriteBlocks(device, inodeTableStart, inodeTableBlocksPerGroup, blockSize, zeroTable);
        }

        // The bitmaps are final: recount free blocks and inodes per group
        // and stamp the descriptors a second time.
        for (uint g = 0; g < groups; g++)
        {
            uint groupStart = g * blocksPerGroup;
            uint groupBlocks = g == groups - 1 ? totalBlocks - g * blocksPerGroup : blocksPerGroup;
            uint blockBitmapBlock = blockBitmapBlocks[(int)g];
            byte[] blockBitmap = new byte[blockSize];
            ReadBlocks(device, blockBitmapBlock, 1, blockSize, blockBitmap);
            int groupFreeBlocks = 0;
            for (uint b = 0; b < groupBlocks; b++)
            {
                if ((blockBitmap[b / 8] & (1 << (int)(b % 8))) == 0)
                {
                    groupFreeBlocks++;
                }
            }

            uint inodeBitmapBlock = inodeBitmapBlocks[(int)g];
            byte[] inodeBitmap = new byte[blockSize];
            ReadBlocks(device, inodeBitmapBlock, 1, blockSize, inodeBitmap);
            uint inodesInThisGroup = g == groups - 1 ? totalInodes - g * inodesPerGroup : inodesPerGroup;
            int groupFreeInodes = 0;
            for (uint i = 0; i < inodesInThisGroup; i++)
            {
                if ((inodeBitmap[i / 8] & (1 << (int)(i % 8))) == 0)
                {
                    groupFreeInodes++;
                }
            }

            ushort usedDirectories = 0;
            if (g == 0)
            {
                usedDirectories = 1;
            }

            int descriptorOffset = (int)g * Ext2SuperblockLayout.GroupDescSize;
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4), blockBitmapBlock);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4), inodeBitmapBlock);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4), inodeTableStarts[(int)g]);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2), (ushort)groupFreeBlocks);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2), (ushort)groupFreeInodes);
            BitConverter.TryWriteBytes(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), usedDirectories);
        }

        WriteBlocks(device, descriptorStartBlock, descriptorBlocks, blockSize, descriptorTable);

        uint totalFreeBlocks = 0;
        uint totalFreeInodes = 0;
        for (uint g = 0; g < groups; g++)
        {
            int descriptorOffset = (int)g * Ext2SuperblockLayout.GroupDescSize;
            totalFreeBlocks += BitConverter.ToUInt16(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2));
            totalFreeInodes += BitConverter.ToUInt16(descriptorTable.AsSpan(descriptorOffset + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2));
        }

        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FreeBlocksCountOffset, 4), totalFreeBlocks);
        BitConverter.TryWriteBytes(superblock.AsSpan(Ext2SuperblockLayout.FreeInodesCountOffset, 4), totalFreeInodes);
        WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset, superblock);

        // Inode 2 is the root: index 1 within group 0's table.
        uint rootIndex = 1;
        uint rootTable = inodeTableStarts[0];
        uint inodesPerBlock = blockSize / Rev0InodeSize;
        uint tableBlock = rootIndex / inodesPerBlock;
        uint offsetInBlock = (rootIndex % inodesPerBlock) * Rev0InodeSize;
        byte[] inodeBlock = new byte[blockSize];
        ReadBlocks(device, rootTable + tableBlock, 1, blockSize, inodeBlock);
        Span<byte> rootInode = inodeBlock.AsSpan((int)offsetInBlock, Rev0InodeSize);
        const ushort RootMode = (ushort)(Ext2InodeLayout.IFDIR | 0x1FF);
        const ushort RootLinks = 2;
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.ModeOffset, 2), RootMode);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.UidOffset, 2), (ushort)0);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.SizeOffset, 4), blockSize);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.AtimeOffset, 4), now);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.CtimeOffset, 4), now);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.MtimeOffset, 4), now);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.GidOffset, 2), (ushort)0);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.LinksCountOffset, 2), RootLinks);
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.BlocksOffset, 4), blockSize / 512);
        uint rootBlock = inodeTableStarts[0] + inodeTableBlocksPerGroup;
        BitConverter.TryWriteBytes(rootInode.Slice(Ext2InodeLayout.BlockOffset, 4), rootBlock);
        WriteBlocks(device, rootTable + tableBlock, 1, blockSize, inodeBlock);

        // The root directory holds "." (itself) and ".." (itself).
        byte[] directoryBlock = new byte[blockSize];
        BitConverter.TryWriteBytes(directoryBlock.AsSpan(Ext2InodeLayout.DirEntryInodeOffset, 4), Ext2SuperblockLayout.RootInodeNumber);
        int dotLength = (Ext2InodeLayout.DirEntryNameOffset + 1 + 3) & ~3;
        BitConverter.TryWriteBytes(directoryBlock.AsSpan(Ext2InodeLayout.DirEntryRecLenOffset, 2), (ushort)dotLength);
        directoryBlock[Ext2InodeLayout.DirEntryNameLenOffset] = 1;
        directoryBlock[Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        directoryBlock[Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        int dotDotOffset = dotLength;
        BitConverter.TryWriteBytes(directoryBlock.AsSpan(dotDotOffset + Ext2InodeLayout.DirEntryInodeOffset, 4), Ext2SuperblockLayout.RootInodeNumber);
        ushort dotDotLength = (ushort)(blockSize - dotLength);
        BitConverter.TryWriteBytes(directoryBlock.AsSpan(dotDotOffset + Ext2InodeLayout.DirEntryRecLenOffset, 2), dotDotLength);
        directoryBlock[dotDotOffset + Ext2InodeLayout.DirEntryNameLenOffset] = 2;
        directoryBlock[dotDotOffset + Ext2InodeLayout.DirEntryFileTypeOffset] = Ext2InodeLayout.FileTypeDir;
        directoryBlock[dotDotOffset + Ext2InodeLayout.DirEntryNameOffset] = (byte)'.';
        directoryBlock[dotDotOffset + Ext2InodeLayout.DirEntryNameOffset + 1] = (byte)'.';
        WriteBlocks(device, rootBlock, 1, blockSize, directoryBlock);

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
            WriteBytes(device, Ext2SuperblockLayout.SuperblockOffset + Ext2SuperblockLayout.MagicOffset, stackalloc byte[2]);
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
        ulong deviceBlockSize = device.BlockSize;
        ulong lba = (ulong)byteOffset / deviceBlockSize;
        int offsetInBlock = (int)((ulong)byteOffset % deviceBlockSize);
        if (offsetInBlock == 0 && (ulong)data.Length % deviceBlockSize == 0)
        {
            ulong blocks = (ulong)data.Length / deviceBlockSize;
            device.WriteBlock(lba, blocks, data);
        }
        else
        {
            ulong start = lba;
            ulong endByte = (ulong)byteOffset + (ulong)data.Length;
            ulong endLba = (endByte + deviceBlockSize - 1) / deviceBlockSize;
            ulong count = endLba - start;
            byte[] buffer = new byte[count * deviceBlockSize];
            device.ReadBlock(start, count, buffer);
            data.CopyTo(buffer.AsSpan(offsetInBlock, data.Length));
            device.WriteBlock(start, count, buffer);
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
        ulong deviceBlockSize = device.BlockSize;
        ulong byteOffset = (ulong)ext2Block * blockSize;
        ulong lba = byteOffset / deviceBlockSize;
        ulong deviceBlocks = (ulong)count * blockSize / deviceBlockSize;
        device.WriteBlock(lba, deviceBlocks, data);
    }

    /// <summary>
    /// Read <paramref name="count"/> filesystem blocks.
    /// </summary>
    /// <param name="device">Block device to read from.</param>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to read.</param>
    /// <param name="blockSize">Filesystem block size in bytes.</param>
    /// <param name="destination">Destination buffer.</param>
    private static void ReadBlocks(IBlockDevice device, uint ext2Block, uint count, uint blockSize, Span<byte> destination)
    {
        ulong deviceBlockSize = device.BlockSize;
        ulong byteOffset = (ulong)ext2Block * blockSize;
        ulong lba = byteOffset / deviceBlockSize;
        ulong deviceBlocks = (ulong)count * blockSize / deviceBlockSize;
        device.ReadBlock(lba, deviceBlocks, destination);
    }
}
