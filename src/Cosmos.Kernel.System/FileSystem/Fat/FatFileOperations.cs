// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Fat;

internal sealed class FatFileOperations : IFileOperations
{
    private readonly FatSuperblock _superblock;

    public FatFileOperations(FatSuperblock superblock)
    {
        _superblock = superblock;
    }

    public long Read(IVfsOpenFile openFile, Span<byte> buffer)
    {
        if (openFile.Inode is not FatInode inode)
        {
            return 0;
        }

        long position = openFile.Position;
        if (position < 0 || position >= inode.Size)
        {
            return 0;
        }

        long remainingFile = inode.Size - position;
        long toRead = buffer.Length < remainingFile ? buffer.Length : remainingFile;
        if (toRead <= 0)
        {
            return 0;
        }

        List<uint> chain = inode.ResolveChain();
        if (chain.Count == 0)
        {
            return 0;
        }

        uint clusterSize = _superblock.Boot.BytesPerCluster;
        long clusterIndex = position / clusterSize;
        long intraOffset = position % clusterSize;
        long copied = 0;

        Span<byte> clusterBuffer = new byte[clusterSize];

        while (copied < toRead && clusterIndex < chain.Count)
        {
            uint cluster = chain[(int)clusterIndex];
            _superblock.ReadCluster(cluster, clusterBuffer);

            long available = clusterSize - intraOffset;
            long want = toRead - copied;
            long chunk = available < want ? available : want;

            clusterBuffer.Slice((int)intraOffset, (int)chunk).CopyTo(buffer.Slice((int)copied, (int)chunk));
            copied += chunk;
            intraOffset = 0;
            clusterIndex++;
        }

        // Position advancement is the caller's responsibility (matches the
        // Linux VFS convention; VfsFileHandle re-applies it on top of this
        // return value).
        return copied;
    }

    public long Write(IVfsOpenFile openFile, ReadOnlySpan<byte> buffer)
    {
        if (openFile.Inode is not FatInode inode)
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
        // FAT caps file size at uint.MaxValue: past it the size cast
        // would wrap and persist a tiny value after the data clusters
        // were already written.
        if (endPosition > uint.MaxValue)
        {
            return 0;
        }

        // Seeking past EOF: zero-fill the gap before writing, or the
        // unzeroed clusters expose whatever freed data they previously
        // held (holes must read as zero).
        if (position > inode.Size && !_superblock.GrowZeroFilled(inode, (uint)position))
        {
            return 0;
        }

        uint clusterSize = _superblock.Boot.BytesPerCluster;
        long clustersNeeded = (endPosition + clusterSize - 1) / clusterSize;

        if (!EnsureChainLength(inode, clustersNeeded))
        {
            return 0;
        }

        List<uint> chain = inode.ResolveChain();
        long clusterIndex = position / clusterSize;
        long intraOffset = position % clusterSize;
        long written = 0;

        Span<byte> clusterBuffer = new byte[clusterSize];

        while (written < buffer.Length && clusterIndex < chain.Count)
        {
            uint cluster = chain[(int)clusterIndex];

            long available = clusterSize - intraOffset;
            long want = buffer.Length - written;
            long chunk = available < want ? available : want;

            // Only a partial-cluster write needs the read-modify-write.
            if (chunk != clusterSize)
            {
                _superblock.ReadCluster(cluster, clusterBuffer);
            }

            buffer.Slice((int)written, (int)chunk).CopyTo(clusterBuffer.Slice((int)intraOffset, (int)chunk));
            _superblock.WriteCluster(cluster, clusterBuffer);

            written += chunk;
            intraOffset = 0;
            clusterIndex++;
        }

        if (written == 0)
        {
            return 0;
        }

        long newEnd = position + written;
        if (newEnd > inode.Size)
        {
            inode.Size = (uint)newEnd;
        }

        _superblock.UpdateInodeEntry(inode);
        return written;
    }

    public bool Seek(IVfsOpenFile openFile, long offset, SeekWhence whence, out long newPosition)
    {
        if (openFile.Inode is not FatInode inode)
        {
            newPosition = 0;
            return false;
        }

        long basePos = whence switch
        {
            SeekWhence.Set => 0,
            SeekWhence.Cur => openFile.Position,
            SeekWhence.End => inode.Size,
            _ => -1,
        };

        if (basePos < 0)
        {
            newPosition = 0;
            return false;
        }

        long target = basePos + offset;
        if (target < 0)
        {
            newPosition = 0;
            return false;
        }

        openFile.Position = target;
        newPosition = target;
        return true;
    }

    public bool Fsync(IVfsOpenFile openFile)
    {
        if (openFile.Inode is FatInode inode)
        {
            _superblock.UpdateInodeEntry(inode);
        }
        // Fsync is a durability point: per the IBlockDevice contract the
        // FAT and data-cluster writes are only durable after Flush.
        _superblock.Flush();
        return true;
    }

    public void Release(IVfsOpenFile openFile) => Fsync(openFile);

    private bool EnsureChainLength(FatInode inode, long targetCount)
    {
        List<uint> chain = inode.ResolveChain();
        if (chain.Count >= targetCount)
        {
            return true;
        }

        FatTable fat = _superblock.Fat;

        if (chain.Count == 0)
        {
            uint first = fat.AllocateChain(1);
            if (first == 0)
            {
                return false;
            }
            inode.FirstCluster = first;
            chain.Add(first);
        }

        long missing = targetCount - chain.Count;
        if (missing > 0)
        {
            // One ExtendChain call instead of a per-cluster allocate-and-
            // link loop (each Set read-modify-writes the FAT sector once
            // per FAT copy).
            uint oldTail = chain[^1];
            if (fat.ExtendChain(oldTail, (uint)missing) == 0)
            {
                return false;
            }
            // Refresh the inode's cached chain with the new links.
            uint next = fat.Get(oldTail);
            while (fat.IsDataCluster(next) && !fat.IsEndOfChain(next) && chain.Count < targetCount)
            {
                chain.Add(next);
                next = fat.Get(next);
            }
        }

        return chain.Count >= targetCount;
    }
}
