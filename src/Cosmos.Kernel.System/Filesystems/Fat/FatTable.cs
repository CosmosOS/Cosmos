// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>
/// FAT12 / FAT16 / FAT32 table accessor and chain manager. All entry
/// reads and writes go through the underlying <see cref="IBlockDevice"/>
/// using <see cref="FatBootSector"/> geometry. Cluster numbers coming from
/// on-disk metadata are untrusted: accessors treat anything outside the
/// volume's data clusters as end-of-chain and never let it drive I/O.
/// </summary>
internal sealed class FatTable
{
    /// <summary>Entry value marking a free cluster.</summary>
    public const uint FreeCluster = 0x00000000;

    /// <summary>First data-cluster number; clusters 0 and 1 are reserved.</summary>
    public const uint FirstDataCluster = 2;

    /// <summary>Lowest FAT32 entry value of the end-of-chain band.</summary>
    public const uint Fat32EndOfChain = 0x0FFFFFF8;

    /// <summary>FAT32 bad-cluster marker.</summary>
    public const uint Fat32BadCluster = 0x0FFFFFF7;

    /// <summary>Canonical FAT32 end-of-chain value written by this driver; also stamped by the formatter into FAT[1] and the root cluster's FAT entry (fatgen103 §4).</summary>
    internal const uint Fat32EndOfChainValue = 0x0FFFFFFF;

    /// <summary>Lowest FAT16 entry value of the end-of-chain band.</summary>
    private const uint Fat16EndOfChain = 0xFFF8;

    /// <summary>FAT16 bad-cluster marker.</summary>
    private const uint Fat16BadCluster = 0xFFF7;

    /// <summary>Canonical FAT16 end-of-chain value written by this driver.</summary>
    private const uint Fat16EndOfChainValue = 0xFFFF;

    /// <summary>Lowest FAT12 entry value of the end-of-chain band.</summary>
    private const uint Fat12EndOfChain = 0x0FF8;

    /// <summary>FAT12 bad-cluster marker.</summary>
    private const uint Fat12BadCluster = 0x0FF7;

    /// <summary>Canonical FAT12 end-of-chain value written by this driver.</summary>
    private const uint Fat12EndOfChainValue = 0x0FFF;

    /// <summary>FAT32 entries use only the low 28 bits.</summary>
    private const uint Fat32EntryMask = 0x0FFFFFFF;

    /// <summary>FAT32 reserved high bits, preserved on write per fatgen103.</summary>
    private const uint Fat32ReservedMask = 0xF0000000;

    /// <summary>FAT12 entries are 12 bits.</summary>
    private const uint Fat12EntryMask = 0x0FFF;

    /// <summary>Low nibble kept when rewriting the odd half of a FAT12 pair.</summary>
    private const uint Fat12LowNibbleMask = 0x000F;

    /// <summary>High nibble kept when rewriting the even half of a FAT12 pair.</summary>
    private const uint Fat12HighNibbleMask = 0xF000;

    /// <summary>FAT32 entry width in bytes.</summary>
    internal const uint Fat32EntrySize = 4;

    /// <summary>FAT16 entry width in bytes.</summary>
    internal const uint Fat16EntrySize = 2;

    /// <summary>FAT12 packs two 12-bit entries into every three bytes (fatgen103 §4: FATOffset = N + N / 2).</summary>
    internal const uint Fat12EntriesPerPair = 2;

    /// <summary>Bytes holding one FAT12 entry pair (fatgen103 §4).</summary>
    internal const uint Fat12PairBytes = 3;

    /// <summary>Bytes spanned by a single 12-bit FAT12 entry when read as a little-endian word.</summary>
    private const int Fat12EntrySpanBytes = 2;

    /// <summary>Bit position of the odd-numbered FAT12 entry within its 16-bit pair word (fatgen103 §4).</summary>
    private const int Fat12OddEntryShift = 4;

    /// <summary>Shift between the low and high byte of a little-endian 16-bit FAT12 pair word.</summary>
    private const int BitsPerByte = 8;

    /// <summary>Mask isolating the low byte of a 16-bit FAT12 pair word.</summary>
    private const int LowByteMask = 0xFF;

    private readonly IBlockDevice _device;
    private readonly FatBootSector _boot;

    /// <summary>
    /// First cluster number past the last addressable one: the smaller of
    /// the volume's data clusters and what one FAT copy can actually hold,
    /// so no entry access can leave the FAT region.
    /// </summary>
    private readonly uint _clusterLimit;

    /// <summary>Reusable FAT-sector buffer; caches the sector at <see cref="_fatSectorLba"/>.</summary>
    private readonly byte[] _fatSector;

    /// <summary>Reusable second buffer for FAT12 entries straddling a sector boundary.</summary>
    private readonly byte[] _fatSpill;

    /// <summary>LBA currently held by <see cref="_fatSector"/>; ulong.MaxValue = invalid.</summary>
    private ulong _fatSectorLba = ulong.MaxValue;

    private uint _nextFreeHint = FirstDataCluster;

    public FatTable(IBlockDevice device, FatBootSector boot)
    {
        _device = device;
        _boot = boot;
        _fatSector = new byte[boot.BytesPerSector];
        _fatSpill = new byte[boot.BytesPerSector];

        // Entries one FAT copy can hold (FAT12 packs 2 entries per 3 bytes).
        ulong fatBytes = (ulong)boot.FatSectorCount * boot.BytesPerSector;
        ulong fatCapacity = boot.Type switch
        {
            FatType.Fat32 => fatBytes / Fat32EntrySize,
            FatType.Fat16 => fatBytes / Fat16EntrySize,
            _ => fatBytes * Fat12EntriesPerPair / Fat12PairBytes,
        };
        ulong limit = (ulong)boot.ClusterCount + FirstDataCluster;
        if (fatCapacity < limit)
        {
            limit = fatCapacity;
        }
        _clusterLimit = (uint)limit;
    }

    /// <summary>True when <paramref name="cluster"/> addresses a data cluster this volume (and its FAT) actually has.</summary>
    public bool IsDataCluster(uint cluster)
    {
        return cluster >= FirstDataCluster && cluster < _clusterLimit;
    }

    public uint Get(uint cluster)
    {
        // Out-of-range numbers come from corrupt on-disk metadata; treat
        // them as end-of-chain instead of decoding whatever sector the
        // unbounded offset math would land on.
        if (!IsDataCluster(cluster))
        {
            return EndOfChainMarker();
        }

        return _boot.Type switch
        {
            FatType.Fat32 => GetFat32(cluster),
            FatType.Fat16 => GetFat16(cluster),
            FatType.Fat12 => GetFat12(cluster),
            _ => 0,
        };
    }

    public void Set(uint cluster, uint value)
    {
        // Never let a corrupt cluster number drive a read-modify-write
        // outside the FAT region (it would be mirrored NumberOfFats times).
        if (!IsDataCluster(cluster))
        {
            return;
        }

        switch (_boot.Type)
        {
            case FatType.Fat32:
                SetFat32(cluster, value);
                break;
            case FatType.Fat16:
                SetFat16(cluster, value);
                break;
            case FatType.Fat12:
                SetFat12(cluster, value);
                break;
        }

        if (value == FreeCluster && cluster >= FirstDataCluster && cluster < _nextFreeHint)
        {
            _nextFreeHint = cluster;
        }
    }

    public bool IsEndOfChain(uint entry)
    {
        return _boot.Type switch
        {
            FatType.Fat32 => entry >= Fat32EndOfChain,
            FatType.Fat16 => entry >= Fat16EndOfChain,
            FatType.Fat12 => entry >= Fat12EndOfChain,
            _ => true,
        };
    }

    public bool IsBadCluster(uint entry)
    {
        return _boot.Type switch
        {
            FatType.Fat32 => entry == Fat32BadCluster,
            FatType.Fat16 => entry == Fat16BadCluster,
            FatType.Fat12 => entry == Fat12BadCluster,
            _ => false,
        };
    }

    public uint EndOfChainMarker()
    {
        return _boot.Type switch
        {
            FatType.Fat32 => Fat32EndOfChainValue,
            FatType.Fat16 => Fat16EndOfChainValue,
            FatType.Fat12 => Fat12EndOfChainValue,
            _ => 0,
        };
    }

    /// <summary>
    /// Walk the chain starting at <paramref name="firstCluster"/>. Stops on
    /// EOC, free, or bad-cluster markers, truncates at links leaving the
    /// volume's data clusters, and bails out if the chain exceeds the
    /// cluster count to defend against loops.
    /// </summary>
    public List<uint> GetChain(uint firstCluster)
    {
        List<uint> chain = new();
        uint current = firstCluster;
        uint guard = _boot.ClusterCount + FirstDataCluster;

        while (IsDataCluster(current) && !IsEndOfChain(current) && !IsBadCluster(current))
        {
            chain.Add(current);
            uint next = Get(current);
            if (next == current || chain.Count > guard)
            {
                break;
            }
            current = next;
        }

        return chain;
    }

    /// <summary>Allocate a chain of <paramref name="count"/> contiguous-by-FAT clusters; returns first cluster, or 0 on failure.</summary>
    public uint AllocateChain(uint count)
    {
        if (count == 0)
        {
            return 0;
        }

        uint first = 0;
        uint previous = 0;

        for (uint i = 0; i < count; i++)
        {
            uint cluster = FindFree();
            if (cluster == 0)
            {
                if (first != 0)
                {
                    Free(first);
                }
                return 0;
            }

            // Reserve immediately so the next FindFree skips it.
            Set(cluster, EndOfChainMarker());

            if (first == 0)
            {
                first = cluster;
            }

            if (previous != 0)
            {
                Set(previous, cluster);
            }

            previous = cluster;
        }

        return first;
    }

    /// <summary>Append <paramref name="count"/> clusters to the chain whose tail is <paramref name="lastCluster"/>; returns new tail or 0 on failure.</summary>
    public uint ExtendChain(uint lastCluster, uint count)
    {
        uint added = AllocateChain(count);
        if (added == 0)
        {
            return 0;
        }

        Set(lastCluster, added);
        uint tail = added;
        while (true)
        {
            uint next = Get(tail);
            if (IsEndOfChain(next))
            {
                return tail;
            }
            tail = next;
        }
    }

    /// <summary>Mark every cluster in the chain rooted at <paramref name="firstCluster"/> as free.</summary>
    public void Free(uint firstCluster)
    {
        List<uint> chain = GetChain(firstCluster);
        for (int i = 0; i < chain.Count; i++)
        {
            Set(chain[i], FreeCluster);
        }
    }

    public uint FindFree()
    {
        uint upper = _clusterLimit;
        uint start = _nextFreeHint < FirstDataCluster ? FirstDataCluster : _nextFreeHint;

        for (uint i = start; i < upper; i++)
        {
            if (Get(i) == FreeCluster)
            {
                _nextFreeHint = i + 1;
                return i;
            }
        }

        // Wrap and search from the beginning in case earlier clusters were freed.
        for (uint i = FirstDataCluster; i < start; i++)
        {
            if (Get(i) == FreeCluster)
            {
                _nextFreeHint = i + 1;
                return i;
            }
        }

        return 0;
    }

    public uint CountFree()
    {
        // Per-entry reads through Get() cost one cached-sector lookup per
        // entry; the FAT12 entry straddling makes a sector sweep fiddly,
        // so FAT12 keeps the per-entry path (the sector cache makes it one
        // read per FAT sector anyway).
        if (_boot.Type == FatType.Fat12)
        {
            uint count = 0;
            for (uint i = FirstDataCluster; i < _clusterLimit; i++)
            {
                if (Get(i) == FreeCluster)
                {
                    count++;
                }
            }
            return count;
        }

        uint entrySize = _boot.Type == FatType.Fat32 ? Fat32EntrySize : Fat16EntrySize;
        uint entriesPerSector = _boot.BytesPerSector / entrySize;
        uint freeCount = 0;
        Span<byte> buffer = _fatSpill;

        for (uint sectorIdx = 0; sectorIdx < _boot.FatSectorCount; sectorIdx++)
        {
            _device.ReadBlock(_boot.FatStartLba + sectorIdx, 1, buffer);
            for (uint j = 0; j < entriesPerSector; j++)
            {
                uint cluster = sectorIdx * entriesPerSector + j;
                if (!IsDataCluster(cluster))
                {
                    continue;
                }

                uint entry = entrySize == Fat32EntrySize
                    ? BitConverter.ToUInt32(buffer.Slice((int)(j * Fat32EntrySize), (int)Fat32EntrySize)) & Fat32EntryMask
                    : BitConverter.ToUInt16(buffer.Slice((int)(j * Fat16EntrySize), (int)Fat16EntrySize));
                if (entry == FreeCluster)
                {
                    freeCount++;
                }
            }
        }
        return freeCount;
    }

    /// <summary>
    /// Returns the cached FAT sector at <paramref name="lba"/>, reading it
    /// from the device only when the cache holds a different sector.
    /// Consecutive entry accesses in the same sector cost no I/O and no
    /// allocation; writers update the buffer in place and write through,
    /// so the cache stays coherent.
    /// </summary>
    private Span<byte> LoadFatSector(ulong lba)
    {
        Span<byte> sector = _fatSector;
        if (_fatSectorLba != lba)
        {
            _device.ReadBlock(lba, 1, sector);
            _fatSectorLba = lba;
        }
        return sector;
    }

    private uint GetFat32(uint cluster)
    {
        uint fatOffset = cluster * Fat32EntrySize;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;

        Span<byte> buffer = LoadFatSector(sectorNumber);
        return BitConverter.ToUInt32(buffer.Slice((int)entryOffset, (int)Fat32EntrySize)) & Fat32EntryMask;
    }

    private void SetFat32(uint cluster, uint value)
    {
        uint fatOffset = cluster * Fat32EntrySize;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;

        Span<byte> buffer = LoadFatSector(sectorNumber);
        uint existing = BitConverter.ToUInt32(buffer.Slice((int)entryOffset, (int)Fat32EntrySize));
        uint merged = (existing & Fat32ReservedMask) | (value & Fat32EntryMask);
        BitConverter.TryWriteBytes(buffer.Slice((int)entryOffset, (int)Fat32EntrySize), merged);

        for (uint i = 0; i < _boot.NumberOfFats; i++)
        {
            _device.WriteBlock(sectorNumber + i * _boot.FatSectorCount, 1, buffer);
        }
    }

    private uint GetFat16(uint cluster)
    {
        uint fatOffset = cluster * Fat16EntrySize;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;

        Span<byte> buffer = LoadFatSector(sectorNumber);
        return BitConverter.ToUInt16(buffer.Slice((int)entryOffset, (int)Fat16EntrySize));
    }

    private void SetFat16(uint cluster, uint value)
    {
        uint fatOffset = cluster * Fat16EntrySize;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;

        Span<byte> buffer = LoadFatSector(sectorNumber);
        BitConverter.TryWriteBytes(buffer.Slice((int)entryOffset, (int)Fat16EntrySize), (ushort)value);

        for (uint i = 0; i < _boot.NumberOfFats; i++)
        {
            _device.WriteBlock(sectorNumber + i * _boot.FatSectorCount, 1, buffer);
        }
    }

    private uint GetFat12(uint cluster)
    {
        uint fatOffset = cluster + cluster / Fat12EntriesPerPair;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;

        Span<byte> buffer = LoadFatSector(sectorNumber);

        ushort word;
        if (entryOffset == _boot.BytesPerSector - 1)
        {
            Span<byte> next = _fatSpill;
            _device.ReadBlock(sectorNumber + 1, 1, next);
            word = (ushort)(buffer[(int)entryOffset] | (next[0] << BitsPerByte));
        }
        else
        {
            word = BitConverter.ToUInt16(buffer.Slice((int)entryOffset, Fat12EntrySpanBytes));
        }

        return (cluster & 1) != 0 ? (uint)(word >> Fat12OddEntryShift) : word & Fat12EntryMask;
    }

    private void SetFat12(uint cluster, uint value)
    {
        uint fatOffset = cluster + cluster / Fat12EntriesPerPair;
        uint sectorNumber = _boot.FatStartLba + fatOffset / _boot.BytesPerSector;
        uint entryOffset = fatOffset % _boot.BytesPerSector;
        bool spans = entryOffset == _boot.BytesPerSector - 1;

        Span<byte> buffer = LoadFatSector(sectorNumber);

        Span<byte> next = Span<byte>.Empty;
        if (spans)
        {
            next = _fatSpill;
            _device.ReadBlock(sectorNumber + 1, 1, next);
        }

        byte low = buffer[(int)entryOffset];
        byte high = spans ? next[0] : buffer[(int)entryOffset + 1];
        ushort word = (ushort)(low | (high << BitsPerByte));

        if ((cluster & 1) != 0)
        {
            word = (ushort)(((uint)word & Fat12LowNibbleMask) | ((value & Fat12EntryMask) << Fat12OddEntryShift));
        }
        else
        {
            word = (ushort)(((uint)word & Fat12HighNibbleMask) | (value & Fat12EntryMask));
        }

        buffer[(int)entryOffset] = (byte)(word & LowByteMask);
        if (spans)
        {
            next[0] = (byte)(word >> BitsPerByte);
        }
        else
        {
            buffer[(int)entryOffset + 1] = (byte)(word >> BitsPerByte);
        }

        for (uint i = 0; i < _boot.NumberOfFats; i++)
        {
            ulong fatBase = sectorNumber + i * _boot.FatSectorCount;
            _device.WriteBlock(fatBase, 1, buffer);
            if (spans)
            {
                _device.WriteBlock(fatBase + 1, 1, next);
            }
        }
    }
}
