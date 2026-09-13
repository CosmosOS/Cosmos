// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Fat;

internal sealed class FatSuperblock : IVfsSuperblock
{
    /// <summary>Directory slots the 8.3 short entry itself occupies — every LFN entry set ends with exactly one (fatgen103 LFN spec).</summary>
    private const int ShortEntrySlotCount = 1;

    private readonly IBlockDevice _device;
    private readonly Dictionary<uint, FatInode> _inodeCache = new();

    public FatBootSector Boot { get; }
    public FatTable Fat { get; }
    public FatInodeOperations InodeOps { get; }
    public FatFileOperations FileOps { get; }
    public FatSuperblockOperations SuperOps { get; }

    public IVfsInode Root { get; }
    public ISuperblockOperations SuperOperations => SuperOps;
    public long BlockSize => Boot.BytesPerSector;
    public ulong MaxNameLength => FatDirectory.MaxLfnNameLength;

    public FatSuperblock(IBlockDevice device, FatBootSector boot)
    {
        _device = device;
        Boot = boot;
        Fat = new FatTable(device, boot);
        InodeOps = new FatInodeOperations(this);
        FileOps = new FatFileOperations(this);
        SuperOps = new FatSuperblockOperations();

        uint rootCluster = boot.Type == FatType.Fat32 ? boot.RootCluster : FatDirectory.EmptyFirstCluster;
        FatInode root = new(this, "/", FatAttr.Directory, rootCluster, FatDirectory.DirectorySizeOnDisk, parent: null, dirEntryByteOffset: FatInode.NoDirEntryOffset, dirEntrySlotCount: 0);
        Root = root;
    }

    public void Drop()
    {
        _inodeCache.Clear();
    }

    /// <summary>Flush the device's volatile write cache — the durability point for sync and unmount.</summary>
    public void Flush()
    {
        _device.Flush();
    }

    public void ReadCluster(uint cluster, Span<byte> data)
    {
        ulong lba = Boot.ClusterToLba(cluster);
        _device.ReadBlock(lba, Boot.SectorsPerCluster, data);
    }

    public void WriteCluster(uint cluster, ReadOnlySpan<byte> data)
    {
        ulong lba = Boot.ClusterToLba(cluster);
        _device.WriteBlock(lba, Boot.SectorsPerCluster, data);
    }

    public byte[] ReadDirectoryData(FatInode dir)
    {
        if (dir.IsFixedRoot)
        {
            byte[] data = new byte[Boot.RootSectorCount * Boot.BytesPerSector];
            _device.ReadBlock(Boot.RootStartLba, Boot.RootSectorCount, data);
            return data;
        }

        List<uint> chain = dir.ResolveChain();
        byte[] full = new byte[chain.Count * Boot.BytesPerCluster];
        Span<byte> buffer = full;
        for (int i = 0; i < chain.Count; i++)
        {
            ReadCluster(chain[i], buffer.Slice(i * (int)Boot.BytesPerCluster, (int)Boot.BytesPerCluster));
        }
        return full;
    }

    public void WriteDirectoryData(FatInode dir, ReadOnlySpan<byte> data)
    {
        if (dir.IsFixedRoot)
        {
            _device.WriteBlock(Boot.RootStartLba, Boot.RootSectorCount, data);
            return;
        }

        List<uint> chain = dir.ResolveChain();
        for (int i = 0; i < chain.Count; i++)
        {
            WriteCluster(chain[i], data.Slice(i * (int)Boot.BytesPerCluster, (int)Boot.BytesPerCluster));
        }
    }

    public FatInode GetOrCreateInode(FatInode parent, FatDirEntry entry)
    {
        if (entry.FirstCluster >= FatTable.FirstDataCluster && _inodeCache.TryGetValue(entry.FirstCluster, out FatInode? cached))
        {
            cached.Name = entry.Name;
            cached.Size = entry.Size;
            cached.Attributes = entry.Attributes;
            cached.Parent = parent;
            cached.DirEntryByteOffset = entry.ByteOffset;
            cached.DirEntrySlotCount = entry.LfnEntryCount + ShortEntrySlotCount;
            return cached;
        }

        FatInode node = new(
            this,
            entry.Name,
            entry.Attributes,
            entry.FirstCluster,
            entry.Size,
            parent,
            entry.ByteOffset,
            entry.LfnEntryCount + ShortEntrySlotCount);

        if (entry.FirstCluster >= FatTable.FirstDataCluster)
        {
            _inodeCache[entry.FirstCluster] = node;
        }
        return node;
    }

    public void ForgetInode(uint firstCluster)
    {
        if (firstCluster >= FatTable.FirstDataCluster)
        {
            _inodeCache.Remove(firstCluster);
        }
    }

    public bool FindChildEntry(FatInode parent, ReadOnlySpan<char> name, out FatDirEntry? match)
    {
        match = null;
        byte[] data = ReadDirectoryData(parent);
        List<FatDirEntry> entries = FatDirectory.Parse(data, Boot.Type == FatType.Fat32);
        string target = name.ToString();
        for (int i = 0; i < entries.Count; i++)
        {
            FatDirEntry entry = entries[i];
            if (entry.IsVolumeId)
            {
                continue;
            }
            if (FatDirectory.NameEqualsIgnoreCase(entry.Name, target) || FatDirectory.NameEqualsIgnoreCase(entry.ShortName, target))
            {
                match = entry;
                return true;
            }
        }
        return false;
    }

    public bool IsDirectoryEmpty(FatInode dir)
    {
        byte[] data = ReadDirectoryData(dir);
        List<FatDirEntry> entries = FatDirectory.Parse(data, Boot.Type == FatType.Fat32);
        for (int i = 0; i < entries.Count; i++)
        {
            FatDirEntry entry = entries[i];
            if (entry.IsVolumeId)
            {
                continue;
            }
            if (entry.Name == "." || entry.Name == "..")
            {
                continue;
            }
            return false;
        }
        return true;
    }

    public bool AllocateDirectoryEntry(
        FatInode parent,
        ReadOnlySpan<char> name,
        FatAttr attr,
        uint firstCluster,
        uint size,
        [NotNullWhen(true)] out FatInode? created)
    {
        created = null;
        string longName = name.ToString();
        if (string.IsNullOrEmpty(longName) || longName.Length > FatDirectory.MaxLfnNameLength)
        {
            return false;
        }

        int lfnCount = FatDirectory.LfnEntryCountFor(longName);
        int slots = lfnCount + ShortEntrySlotCount;

        byte[] data = ReadDirectoryData(parent);
        int slot = FatDirectory.FindFreeRun(data, slots, out bool consumedTerminator);
        if (slot < 0)
        {
            if (parent.IsFixedRoot)
            {
                return false;
            }
            data = GrowDirectory(parent, data, slots);
            slot = FatDirectory.FindFreeRun(data, slots, out consumedTerminator);
            if (slot < 0)
            {
                return false;
            }
        }

        Span<char> shortBuffer = stackalloc char[FatDirectory.ShortNameLength];
        FatDirectory.BuildShortName(longName, shortBuffer, data);

        if (lfnCount > 0)
        {
            FatDirectory.WriteLfnEntries(data, slot, longName, shortBuffer);
        }
        int eightThreeOffset = slot + lfnCount * FatDirectory.EntrySize;
        FatDirectory.WriteShortEntry(data, eightThreeOffset, shortBuffer, attr, firstCluster, size);

        // The run swallowed the 0x00 terminator: re-terminate after the
        // new entries, or stale bytes beyond get parsed back to life.
        if (consumedTerminator)
        {
            int terminatorOffset = slot + slots * FatDirectory.EntrySize;
            if (terminatorOffset + FatDirectory.EntrySize <= data.Length)
            {
                data.AsSpan(terminatorOffset, FatDirectory.EntrySize).Clear();
            }
        }

        WriteDirectoryData(parent, data);

        FatInode node = new(
            this,
            longName,
            attr,
            firstCluster,
            size,
            parent,
            eightThreeOffset,
            slots);
        if (firstCluster >= FatTable.FirstDataCluster)
        {
            _inodeCache[firstCluster] = node;
        }
        created = node;
        return true;
    }

    public void RemoveDirectoryEntry(FatInode parent, FatDirEntry match)
    {
        byte[] data = ReadDirectoryData(parent);
        int slots = match.LfnEntryCount + ShortEntrySlotCount;
        int start = match.ByteOffset - match.LfnEntryCount * FatDirectory.EntrySize;
        FatDirectory.MarkDeleted(data, start, slots);
        WriteDirectoryData(parent, data);

        // A live inode may still be referenced by an open handle; its
        // later Fsync must not patch whatever entry reuses this slot
        // (cross-link corruption of the new file). Invalidate the offset
        // while the cache can still locate the inode.
        if (match.FirstCluster >= FatTable.FirstDataCluster
            && _inodeCache.TryGetValue(match.FirstCluster, out FatInode? live)
            && live.DirEntryByteOffset == match.ByteOffset)
        {
            live.DirEntryByteOffset = FatInode.NoDirEntryOffset;
        }
    }

    public void UpdateInodeEntry(FatInode inode)
    {
        if (inode.Parent == null || inode.DirEntryByteOffset < 0)
        {
            return;
        }

        byte[] data = ReadDirectoryData(inode.Parent);
        int offset = inode.DirEntryByteOffset;
        if (offset + FatDirectory.EntrySize > data.Length)
        {
            return;
        }

        // Defense in depth for handles that escaped the cache-based
        // invalidation: never patch a slot that was deleted or is the
        // directory terminator.
        byte marker = data[offset];
        if (marker == FatDirectory.DeletedMarker || marker == FatDirectory.EndOfDirectoryMarker)
        {
            return;
        }

        BitConverter.TryWriteBytes(data.AsSpan(offset + FatDirectory.FirstClusterHighOffset, FatDirectory.ClusterWordBytes), (ushort)((inode.FirstCluster >> FatDirectory.ClusterHighShift) & FatDirectory.ClusterWordMask));
        BitConverter.TryWriteBytes(data.AsSpan(offset + FatDirectory.FirstClusterLowOffset, FatDirectory.ClusterWordBytes), (ushort)(inode.FirstCluster & FatDirectory.ClusterWordMask));
        BitConverter.TryWriteBytes(data.AsSpan(offset + FatDirectory.SizeOffset, FatDirectory.SizeFieldBytes), inode.Size);
        data[offset + FatDirectory.AttributesOffset] = (byte)inode.Attributes;
        WriteDirectoryData(inode.Parent, data);

        // Files created empty enter the world with cluster 0 and are never
        // cached; index the live inode once a first cluster exists so a
        // later unlink can invalidate this object's slot reference.
        if (inode.FirstCluster >= FatTable.FirstDataCluster)
        {
            _inodeCache[inode.FirstCluster] = inode;
        }
    }

    public void TruncateChain(FatInode inode, uint newSize)
    {
        uint clusterSize = Boot.BytesPerCluster;
        uint clustersToKeep = (newSize + clusterSize - 1) / clusterSize;

        List<uint> chain = inode.ResolveChain();
        if (clustersToKeep == 0)
        {
            if (inode.FirstCluster >= FatTable.FirstDataCluster)
            {
                Fat.Free(inode.FirstCluster);
            }
            inode.FirstCluster = FatDirectory.EmptyFirstCluster;
            chain.Clear();
            return;
        }

        if (chain.Count <= clustersToKeep)
        {
            return;
        }

        uint newTail = chain[(int)clustersToKeep - 1];
        for (int i = (int)clustersToKeep; i < chain.Count; i++)
        {
            Fat.Set(chain[i], FatTable.FreeCluster);
        }
        Fat.Set(newTail, Fat.EndOfChainMarker());
        chain.RemoveRange((int)clustersToKeep, chain.Count - (int)clustersToKeep);
    }

    public bool GrowZeroFilled(FatInode inode, uint newSize)
    {
        Span<byte> zeroCluster = new byte[Boot.BytesPerCluster];

        uint clusterSize = Boot.BytesPerCluster;
        long clustersNeeded = ((long)newSize + clusterSize - 1) / clusterSize;
        List<uint> chain = inode.ResolveChain();
        int preexisting = chain.Count;

        while (chain.Count < clustersNeeded)
        {
            uint added = Fat.AllocateChain(1);
            if (added == 0)
            {
                return false;
            }
            if (chain.Count == 0)
            {
                inode.FirstCluster = added;
            }
            else
            {
                Fat.Set(chain[^1], added);
            }
            chain.Add(added);
            WriteCluster(added, zeroCluster);
        }

        // Zero the gap between the old EOF and newSize. Freshly allocated
        // clusters are already zeroed above; only the partial cluster
        // holding the old EOF needs a read-modify-write, full pre-existing
        // clusters get the zero buffer written directly.
        Span<byte> rmwBuffer = Span<byte>.Empty;
        long pos = inode.Size;
        while (pos < newSize)
        {
            long clusterIndex = pos / clusterSize;
            long intra = pos % clusterSize;
            long clear = clusterSize - intra;
            if (pos + clear > newSize)
            {
                clear = newSize - pos;
            }

            if (clusterIndex >= preexisting)
            {
                pos += clear;
                continue;
            }

            uint cluster = chain[(int)clusterIndex];
            if (intra == 0 && clear == clusterSize)
            {
                WriteCluster(cluster, zeroCluster);
            }
            else
            {
                if (rmwBuffer.IsEmpty)
                {
                    rmwBuffer = new byte[Boot.BytesPerCluster];
                }
                ReadCluster(cluster, rmwBuffer);
                rmwBuffer.Slice((int)intra, (int)clear).Clear();
                WriteCluster(cluster, rmwBuffer);
            }
            pos += clear;
        }

        inode.Size = newSize;
        return true;
    }

    private byte[] GrowDirectory(FatInode parent, byte[] currentData, int slotsNeeded)
    {
        if (parent.IsFixedRoot)
        {
            return currentData;
        }

        // One cluster may not fit the requested run: a maximal LFN needs
        // 21 slots (672 bytes) but a 512-byte cluster holds only 16.
        uint clustersNeeded =
            ((uint)slotsNeeded * (uint)FatDirectory.EntrySize + Boot.BytesPerCluster - 1) / Boot.BytesPerCluster;
        if (clustersNeeded == 0)
        {
            clustersNeeded = 1;
        }

        List<uint> chain = parent.ResolveChain();
        uint added = Fat.AllocateChain(clustersNeeded);
        if (added == 0)
        {
            return currentData;
        }

        if (chain.Count == 0)
        {
            parent.FirstCluster = added;
            // Persist the new first cluster, or the parent's on-disk
            // entry keeps cluster 0 and the children are lost on remount.
            UpdateInodeEntry(parent);
        }
        else
        {
            Fat.Set(chain[^1], added);
        }

        // chain is the inode's cached list: append the new clusters so the
        // cache stays coherent with the FAT links written above.
        List<uint> newClusters = Fat.GetChain(added);
        Span<byte> emptyCluster = new byte[Boot.BytesPerCluster];
        for (int i = 0; i < newClusters.Count; i++)
        {
            chain.Add(newClusters[i]);
            WriteCluster(newClusters[i], emptyCluster);
        }

        byte[] grown = new byte[currentData.Length + newClusters.Count * (int)Boot.BytesPerCluster];
        Buffer.BlockCopy(currentData, 0, grown, 0, currentData.Length);
        return grown;
    }

}
