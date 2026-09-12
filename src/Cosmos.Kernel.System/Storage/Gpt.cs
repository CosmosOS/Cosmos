// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// GPT (GUID Partition Table) parser / writer. Reads / writes the primary
/// header at LBA 1 plus the partition entry array starting at LBA 2.
/// 512-byte sectors assumed (matches every block device this kernel
/// currently exposes).
/// </summary>
public static class Gpt
{
    /// <summary>"EFI PART" little-endian.</summary>
    private const ulong EfiPartSignature = 0x5452415020494645UL;

    /// <summary>
    /// Upper bound on the on-disk NumberOfPartitionEntries field this
    /// parser will honor (8x the standard 128). This format writes CRC32s
    /// as 0, so header corruption is undetectable — an unclamped count
    /// (up to 2^32) would drive the boot-time partition scan through
    /// millions of sector reads.
    /// </summary>
    private const uint MaxEntryCount = 1024;

    /// <summary>GPT header field offset of the "EFI PART" signature (UEFI spec, byte 0).</summary>
    private const int HeaderSignatureOffset = 0;

    /// <summary>GPT header field offset of the revision (UEFI spec, byte 8).</summary>
    private const int HeaderRevisionOffset = 8;

    /// <summary>GPT header field offset of the header size (UEFI spec, byte 12).</summary>
    private const int HeaderSizeOffset = 12;

    /// <summary>GPT header field offset of the header CRC32 (UEFI spec, byte 16).</summary>
    private const int HeaderCrcOffset = 16;

    /// <summary>GPT header field offset of the current (my) LBA (UEFI spec, byte 24).</summary>
    private const int HeaderCurrentLbaOffset = 24;

    /// <summary>GPT header field offset of the backup header LBA (UEFI spec, byte 32).</summary>
    private const int HeaderBackupLbaOffset = 32;

    /// <summary>GPT header field offset of the first usable LBA (UEFI spec, byte 40).</summary>
    private const int HeaderFirstUsableOffset = 40;

    /// <summary>GPT header field offset of the last usable LBA (UEFI spec, byte 48).</summary>
    private const int HeaderLastUsableOffset = 48;

    /// <summary>GPT header field offset of the disk GUID (UEFI spec, byte 56).</summary>
    private const int HeaderDiskGuidOffset = 56;

    /// <summary>GPT header field offset of the partition entry array starting LBA (UEFI spec, byte 72).</summary>
    private const int HeaderEntryArrayLbaOffset = 72;

    /// <summary>GPT header field offset of the number of partition entries (UEFI spec, byte 80).</summary>
    private const int HeaderEntryCountOffset = 80;

    /// <summary>GPT header field offset of the size of one partition entry (UEFI spec, byte 84).</summary>
    private const int HeaderEntrySizeOffset = 84;

    /// <summary>GPT header field offset of the partition entry array CRC32 (UEFI spec, byte 88).</summary>
    private const int HeaderEntryArrayCrcOffset = 88;

    /// <summary>GPT revision 1.0 as encoded in the header (0x00010000).</summary>
    private const uint GptRevision = 0x00010000u;

    /// <summary>Size in bytes of the GPT header structure (UEFI spec: 92).</summary>
    private const uint GptHeaderSizeBytes = 92u;

    /// <summary>Placeholder stamped into the header and entry-array CRC32 fields (UEFI spec 5.3.2) — this writer does not compute checksums.</summary>
    private const uint UncomputedCrc32 = 0u;

    /// <summary>Standard GPT partition entry size in bytes (UEFI spec); also the minimum SizeOfPartitionEntry accepted when parsing.</summary>
    private const uint PartitionEntrySizeBytes = 128u;

    /// <summary>Standard number of partition entries written to a fresh GPT (UEFI spec minimum array of 128 entries).</summary>
    private const uint DefaultPartitionEntryCount = 128u;

    /// <summary>Partition entry field offset of the unique partition GUID (UEFI spec, byte 16).</summary>
    private const int EntryUniqueGuidOffset = 16;

    /// <summary>Partition entry field offset of the first (starting) LBA (UEFI spec, byte 32).</summary>
    private const int EntryFirstLbaOffset = 32;

    /// <summary>Partition entry field offset of the last (ending, inclusive) LBA (UEFI spec, byte 40).</summary>
    private const int EntryLastLbaOffset = 40;

    /// <summary>Partition entry field offset of the attribute flags (UEFI spec, byte 48).</summary>
    private const int EntryAttributesOffset = 48;

    /// <summary>Attribute flags stamped on a freshly added partition entry: none set (UEFI spec 5.3.3).</summary>
    private const ulong NoEntryAttributes = 0UL;

    /// <summary>Width in bytes of a 64-bit on-disk field (signature, LBA values).</summary>
    private const int UInt64FieldSize = 8;

    /// <summary>Width in bytes of a 32-bit on-disk field (revision, sizes, counts, CRCs).</summary>
    private const int UInt32FieldSize = 4;

    /// <summary>Width in bytes of an on-disk GUID field.</summary>
    private const int GuidFieldSize = 16;

    /// <summary>
    /// LBA of the primary GPT header (sector 1, UEFI spec). Public so MBR
    /// creation and tests wipe the same sector this writer stamps,
    /// mirroring <see cref="FirstUsableLba"/>.
    /// </summary>
    public const ulong PrimaryHeaderLba = 1;

    /// <summary>LBA where the partition entry array starts in the standard layout (first sector after the protective MBR and primary header); also the minimum PartitionEntryLBA accepted when parsing.</summary>
    private const ulong EntryArrayLba = 2;

    /// <summary>
    /// First usable LBA in the standard layout: protective MBR + header +
    /// 32 entry-array sectors occupy LBAs 0..33. Public so shell tooling
    /// validates against the same value the writer stamps.
    /// </summary>
    public const ulong FirstUsableLba = 34;

    /// <summary>Minimum device size in sectors for a GPT to exist at all (LBA 0 plus the header at LBA 1).</summary>
    private const ulong MinGptBlockCount = 2;

    /// <summary>Minimum device size in sectors accepted by Create: LBAs 0..33 plus a usable area and a backup slot at BlockCount-1.</summary>
    private const ulong MinCreateBlockCount = 64;

    /// <summary>Starting CHS head of the protective partition (head 0; with sector 2 and cylinder 0 this addresses LBA 1, UEFI spec 5.2.3).</summary>
    private const byte ProtectiveMbrStartChsHead = 0x00;

    /// <summary>Starting CHS sector of the protective partition (sector numbering is 1-based; 2 maps to LBA 1).</summary>
    private const byte ProtectiveMbrStartChsSector = 0x02;

    /// <summary>Starting CHS cylinder of the protective partition (cylinder 0, UEFI spec 5.2.3).</summary>
    private const byte ProtectiveMbrStartChsCylinder = 0x00;

    /// <summary>
    /// First absolute LBA of the protective partition (LBA 1, right after
    /// the MBR; UEFI spec 5.2.3). Public so tests craft protective entries
    /// with the same value this writer stamps.
    /// </summary>
    public const uint ProtectiveMbrStartLba = 1u;

    /// <summary>Byte offset of the first 32-bit word inside a 16-byte GUID.</summary>
    private const int GuidDword0Offset = 0;

    /// <summary>Byte offset of the second 32-bit word inside a 16-byte GUID.</summary>
    private const int GuidDword1Offset = 4;

    /// <summary>Byte offset of the third 32-bit word inside a 16-byte GUID.</summary>
    private const int GuidDword2Offset = 8;

    /// <summary>Byte offset of the fourth 32-bit word inside a 16-byte GUID.</summary>
    private const int GuidDword3Offset = 12;

    /// <summary>XOR salt mixed into the second GUID dword so deterministic GUIDs differ per word.</summary>
    private const ulong GuidMixSalt1 = 0x12345678UL;

    /// <summary>XOR salt mixed into the third GUID dword so deterministic GUIDs differ per word.</summary>
    private const ulong GuidMixSalt2 = 0x87654321UL;

    /// <summary>XOR salt mixed into the fourth GUID dword so deterministic GUIDs differ per word.</summary>
    private const ulong GuidMixSalt3 = 0xDEADBEEFUL;

    /// <summary>Microsoft Basic Data Partition GUID — used by FAT/NTFS/exFAT volumes.</summary>
    public static readonly Guid BasicDataPartitionType = new(
        0xEBD0A0A2, 0xB9E5, 0x4433, 0x87, 0xC0, 0x68, 0xB6, 0xB7, 0x26, 0x99, 0xC7);

    /// <summary>True if the GPT header at LBA 1 starts with the EFI PART signature.</summary>
    public static bool IsGpt(IBlockDevice device)
    {
        // A GPT needs at least LBA 0 + LBA 1; per the IBlockDevice contract
        // reading past the end throws, and that throw would otherwise leak
        // out of a "is this a GPT?" probe (and kill a boot-time scan).
        if (device.BlockCount < MinGptBlockCount)
        {
            return false;
        }

        Span<byte> header = new byte[device.BlockSize];
        device.ReadBlock(PrimaryHeaderLba, 1, header);
        return BitConverter.ToUInt64(header.Slice(HeaderSignatureOffset, UInt64FieldSize)) == EfiPartSignature;
    }

    /// <summary>
    /// Walk the GPT partition entry array. Empty slots (zero PartitionType
    /// GUID) are skipped.
    /// </summary>
    public static List<GptPartitionEntry> Parse(IBlockDevice device)
    {
        List<GptPartitionEntry> partitions = new();
        if (!TryReadEntryArrayLayout(device, out EntryArrayLayout layout))
        {
            return partitions;
        }

        (ulong entryStartLba, uint entryCount, uint entrySize, uint entriesPerSector, ulong arraySectors) = layout;
        ulong blockSize = device.BlockSize;

        Span<byte> sector = new byte[blockSize];
        for (ulong s = 0; s < (ulong)entryCount; s += entriesPerSector)
        {
            device.ReadBlock(entryStartLba + s / entriesPerSector, 1, sector);
            uint thisSector = (uint)Math.Min((ulong)entriesPerSector, entryCount - s);
            for (uint j = 0; j < thisSector; j++)
            {
                int offset = (int)(j * entrySize);
                Guid partType = ReadGuid(sector.Slice(offset, GuidFieldSize));
                if (partType == Guid.Empty)
                {
                    continue;
                }

                Guid partGuid = ReadGuid(sector.Slice(offset + EntryUniqueGuidOffset, GuidFieldSize));
                ulong startLba = BitConverter.ToUInt64(sector.Slice(offset + EntryFirstLbaOffset, UInt64FieldSize));
                ulong endLba = BitConverter.ToUInt64(sector.Slice(offset + EntryLastLbaOffset, UInt64FieldSize));
                // endLba is inclusive. Reject corrupt entries outright:
                // endLba < startLba would underflow the count to ~2^64, a
                // range past the disk would authorize wild host I/O, and a
                // start inside the GPT structures (protective MBR, header,
                // entry array) would let a partition write corrupt the
                // table itself — CRCs are 0, so nothing else would notice.
                if (endLba < startLba || startLba < entryStartLba + arraySectors || endLba >= device.BlockCount)
                {
                    continue;
                }
                ulong count = endLba + 1 - startLba;

                partitions.Add(new GptPartitionEntry(partType, partGuid, startLba, count));
            }
        }

        return partitions;
    }

    /// <summary>
    /// Write a fresh GPT layout: protective MBR at LBA 0, primary header at
    /// LBA 1, and 32 zeroed sectors of 128-byte partition entries (LBAs
    /// 2..33). Backup header / array intentionally not written — callers
    /// targeting test images don't need them, and writing them needs CRC
    /// support that the boot path doesn't yet ship.
    /// </summary>
    public static void Create(IBlockDevice device)
    {
        ulong blockSize = device.BlockSize;

        // The layout needs LBAs 0..33 plus a usable area and a backup slot
        // at BlockCount-1; smaller devices would underflow the
        // FirstUsable/LastUsable math below.
        if (device.BlockCount < MinCreateBlockCount)
        {
            throw new ArgumentException("Device too small for a GPT layout.", nameof(device));
        }

        // Protective MBR at LBA 0.
        Span<byte> protectiveMbr = new byte[blockSize];
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryStatusOffset] = Mbr.StatusInactive;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryStartChsHeadOffset] = ProtectiveMbrStartChsHead;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryStartChsSectorOffset] = ProtectiveMbrStartChsSector;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryStartChsCylinderOffset] = ProtectiveMbrStartChsCylinder;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntrySystemIdOffset] = Mbr.SystemIdGptProtective;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryEndChsHeadOffset] = Mbr.ChsPlaceholder;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryEndChsSectorOffset] = Mbr.ChsPlaceholder;
        protectiveMbr[Mbr.PartitionTableOffset + Mbr.EntryEndChsCylinderOffset] = Mbr.ChsPlaceholder;
        BitConverter.TryWriteBytes(protectiveMbr.Slice(Mbr.PartitionTableOffset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes), ProtectiveMbrStartLba);
        uint sizeInLba = device.BlockCount > Mbr.LbaFieldMaxValue
            ? Mbr.LbaFieldMaxValue
            : (uint)(device.BlockCount - 1);
        BitConverter.TryWriteBytes(protectiveMbr.Slice(Mbr.PartitionTableOffset + Mbr.EntrySectorCountOffset, Mbr.LbaFieldSizeBytes), sizeInLba);
        BitConverter.TryWriteBytes(protectiveMbr.Slice(Mbr.SignatureOffset, Mbr.SignatureSizeBytes), Mbr.MbrSignature);
        device.WriteBlock(Mbr.MbrSectorLba, 1, protectiveMbr);

        // GPT header at LBA 1.
        Span<byte> header = new byte[blockSize];
        BitConverter.TryWriteBytes(header.Slice(HeaderSignatureOffset, UInt64FieldSize), EfiPartSignature);
        BitConverter.TryWriteBytes(header.Slice(HeaderRevisionOffset, UInt32FieldSize), GptRevision);
        BitConverter.TryWriteBytes(header.Slice(HeaderSizeOffset, UInt32FieldSize), GptHeaderSizeBytes);
        BitConverter.TryWriteBytes(header.Slice(HeaderCrcOffset, UInt32FieldSize), UncomputedCrc32); // not computed; consumers that care will reject
        BitConverter.TryWriteBytes(header.Slice(HeaderCurrentLbaOffset, UInt64FieldSize), PrimaryHeaderLba);
        BitConverter.TryWriteBytes(header.Slice(HeaderBackupLbaOffset, UInt64FieldSize), device.BlockCount - 1);
        BitConverter.TryWriteBytes(header.Slice(HeaderFirstUsableOffset, UInt64FieldSize), FirstUsableLba);
        BitConverter.TryWriteBytes(header.Slice(HeaderLastUsableOffset, UInt64FieldSize), device.BlockCount - FirstUsableLba);
        // Disk GUID — deterministic, derived from disk size so identical inputs yield identical layouts.
        ulong sizeMix = device.BlockCount;
        WriteDeterministicGuid(header.Slice(HeaderDiskGuidOffset, GuidFieldSize), sizeMix);
        BitConverter.TryWriteBytes(header.Slice(HeaderEntryArrayLbaOffset, UInt64FieldSize), EntryArrayLba);
        BitConverter.TryWriteBytes(header.Slice(HeaderEntryCountOffset, UInt32FieldSize), DefaultPartitionEntryCount);
        BitConverter.TryWriteBytes(header.Slice(HeaderEntrySizeOffset, UInt32FieldSize), PartitionEntrySizeBytes);
        BitConverter.TryWriteBytes(header.Slice(HeaderEntryArrayCrcOffset, UInt32FieldSize), UncomputedCrc32);
        device.WriteBlock(PrimaryHeaderLba, 1, header);

        // Zero the partition entry array (LBAs 2..33 with 512B sectors and 128 entries × 128B).
        Span<byte> empty = new byte[blockSize];
        ulong entriesPerSector = blockSize / PartitionEntrySizeBytes;
        ulong arraySectors = entriesPerSector == 0 ? 0 : (DefaultPartitionEntryCount + entriesPerSector - 1) / entriesPerSector;
        for (ulong i = 0; i < arraySectors; i++)
        {
            device.WriteBlock(EntryArrayLba + i, 1, empty);
        }
    }

    /// <summary>
    /// Add a partition entry of <paramref name="partitionType"/> covering
    /// <paramref name="sectorCount"/> sectors starting at
    /// <paramref name="startSector"/>. Returns false if the GPT header is
    /// missing or no slot is free.
    /// </summary>
    public static bool AddPartition(IBlockDevice device, ulong startSector, ulong sectorCount, Guid partitionType)
    {
        if (!TryReadEntryArrayLayout(device, out EntryArrayLayout layout))
        {
            return false;
        }

        // Reject entries this file's own Parse would silently drop (zero
        // length, inside the GPT structures) or that point past the disk:
        // returning true for a partition that never materializes — or
        // stamping a wild range other tooling will trust — helps nobody.
        if (sectorCount == 0 || startSector < FirstUsableLba || startSector >= device.BlockCount
            || sectorCount > device.BlockCount - startSector)
        {
            return false;
        }

        // AddPartition, ResizePartition and MovePartition all refuse a range
        // that overlaps another entry: two entries aliasing the same sectors
        // let a write through one corrupt the other, and with CRCs written
        // as 0 nothing downstream would notice.
        if (OverlapsOtherEntry(device, layout, NoEntryIndex, startSector, sectorCount))
        {
            return false;
        }

        (ulong entryStartLba, uint entryCount, uint entrySize, uint entriesPerSector, _) = layout;
        ulong blockSize = device.BlockSize;

        Span<byte> sector = new byte[blockSize];
        for (uint s = 0; s < entryCount; s += entriesPerSector)
        {
            ulong lba = entryStartLba + s / entriesPerSector;
            device.ReadBlock(lba, 1, sector);
            uint thisSector = (uint)Math.Min((ulong)entriesPerSector, entryCount - s);
            for (uint j = 0; j < thisSector; j++)
            {
                int offset = (int)(j * entrySize);
                if (!IsZero(sector.Slice(offset, GuidFieldSize)))
                {
                    continue;
                }

                WriteGuid(sector.Slice(offset, GuidFieldSize), partitionType);
                ulong slotIdx = s + j;
                ulong guidMix = startSector ^ sectorCount ^ slotIdx;
                WriteDeterministicGuid(sector.Slice(offset + EntryUniqueGuidOffset, GuidFieldSize), guidMix);
                BitConverter.TryWriteBytes(sector.Slice(offset + EntryFirstLbaOffset, UInt64FieldSize), startSector);
                BitConverter.TryWriteBytes(sector.Slice(offset + EntryLastLbaOffset, UInt64FieldSize), startSector + sectorCount - 1);
                BitConverter.TryWriteBytes(sector.Slice(offset + EntryAttributesOffset, UInt64FieldSize), NoEntryAttributes);

                device.WriteBlock(lba, 1, sector);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Mark the <paramref name="index"/>-th non-empty entry as
    /// deleted by zeroing the whole entry — UEFI expects unused entries
    /// fully zeroed, and a stale UTF-16 name would resurface when
    /// <see cref="AddPartition"/> reuses the slot without rewriting it.
    /// </summary>
    public static bool RemovePartition(IBlockDevice device, int index)
    {
        return MutateEntry(device, index, (Span<byte> entry, EntryArrayLayout _) =>
        {
            entry.Clear();
            return true;
        });
    }

    /// <summary>
    /// Rewrite the end LBA of <paramref name="index"/> so the
    /// partition spans <paramref name="newSectorCount"/> sectors. Start LBA /
    /// type / partition GUID preserved. Table-level only — does not adjust
    /// the filesystem inside.
    /// </summary>
    public static bool ResizePartition(IBlockDevice device, int index, ulong newSectorCount)
    {
        if (newSectorCount == 0)
        {
            return false;
        }

        return MutateEntry(device, index, (Span<byte> entry, EntryArrayLayout layout) =>
        {
            ulong startLba = BitConverter.ToUInt64(entry.Slice(EntryFirstLbaOffset, UInt64FieldSize));
            // Same write-time rejection as AddPartition: never stamp a
            // geometry this file's own Parse would drop, and never grow
            // into a neighbour.
            if (newSectorCount > device.BlockCount - startLba
                || OverlapsOtherEntry(device, layout, index, startLba, newSectorCount))
            {
                return false;
            }
            BitConverter.TryWriteBytes(entry.Slice(EntryLastLbaOffset, UInt64FieldSize), startLba + newSectorCount - 1);
            return true;
        });
    }

    /// <summary>
    /// Rewrite the start and end LBAs of <paramref name="index"/> so
    /// the partition lives at <paramref name="newStartSector"/> with the same
    /// length. Table-level only — does not relocate data.
    /// </summary>
    public static bool MovePartition(IBlockDevice device, int index, ulong newStartSector)
    {
        return MutateEntry(device, index, (Span<byte> entry, EntryArrayLayout layout) =>
        {
            ulong startLba = BitConverter.ToUInt64(entry.Slice(EntryFirstLbaOffset, UInt64FieldSize));
            ulong endLba = BitConverter.ToUInt64(entry.Slice(EntryLastLbaOffset, UInt64FieldSize));
            ulong sectorCount = endLba + 1 - startLba;
            // Same write-time rejection as AddPartition: a start inside the
            // GPT structures, a range past the disk end or a range on top
            // of a neighbour must not be stamped into the table.
            if (newStartSector < FirstUsableLba || newStartSector >= device.BlockCount
                || sectorCount > device.BlockCount - newStartSector
                || OverlapsOtherEntry(device, layout, index, newStartSector, sectorCount))
            {
                return false;
            }
            BitConverter.TryWriteBytes(entry.Slice(EntryFirstLbaOffset, UInt64FieldSize), newStartSector);
            BitConverter.TryWriteBytes(entry.Slice(EntryLastLbaOffset, UInt64FieldSize), newStartSector + sectorCount - 1);
            return true;
        });
    }

    /// <summary>
    /// Rewrites one partition entry in place. Receives the entry's 0..55 byte
    /// region and returns false, having written nothing, to abort the mutation.
    /// </summary>
    /// <param name="entry">The entry's 0..55 byte region.</param>
    /// <param name="layout">The validated entry array geometry, for overlap checks against the other entries.</param>
    /// <returns>true when <paramref name="entry"/> was rewritten and should be committed.</returns>
    private delegate bool EntryMutator(Span<byte> entry, EntryArrayLayout layout);

    /// <summary>
    /// Value of an <c>excludeIndex</c> meaning no entry is exempt from the
    /// overlap check, for a partition that does not exist yet.
    /// </summary>
    private const int NoEntryIndex = -1;

    /// <summary>
    /// Geometry of the partition entry array as declared by a validated
    /// primary header: where it starts, how many entries it declares, how
    /// large each is, and how those pack into sectors.
    /// </summary>
    private readonly record struct EntryArrayLayout(
        ulong EntryStartLba,
        uint EntryCount,
        uint EntrySize,
        uint EntriesPerSector,
        ulong ArraySectors);

    /// <summary>
    /// Read the primary header and range-check every field that drives
    /// I/O. Trust nothing beyond the signature: CRC32s are written as 0 by
    /// this format, so corruption is undetectable, a zeroed entry size
    /// would divide by zero, and a wild count or array LBA would drive
    /// unbounded or out-of-range reads.
    /// </summary>
    private static bool TryReadEntryArrayLayout(IBlockDevice device, out EntryArrayLayout layout)
    {
        layout = default;
        if (device.BlockCount < MinGptBlockCount)
        {
            return false;
        }

        ulong blockSize = device.BlockSize;
        Span<byte> header = new byte[blockSize];
        device.ReadBlock(PrimaryHeaderLba, 1, header);
        if (BitConverter.ToUInt64(header.Slice(HeaderSignatureOffset, UInt64FieldSize)) != EfiPartSignature)
        {
            return false;
        }

        ulong entryStartLba = BitConverter.ToUInt64(header.Slice(HeaderEntryArrayLbaOffset, UInt64FieldSize));
        uint entryCount = BitConverter.ToUInt32(header.Slice(HeaderEntryCountOffset, UInt32FieldSize));
        uint entrySize = BitConverter.ToUInt32(header.Slice(HeaderEntrySizeOffset, UInt32FieldSize));
        if (entrySize < PartitionEntrySizeBytes || entrySize > blockSize
            || entryCount > MaxEntryCount
            || entryStartLba < EntryArrayLba || entryStartLba >= device.BlockCount)
        {
            return false;
        }

        uint entriesPerSector = (uint)(blockSize / entrySize);
        if (entriesPerSector == 0)
        {
            return false;
        }

        // Bound the end of the array too: a start LBA near the disk end
        // with a large count would otherwise still drive out-of-range reads.
        ulong arraySectors = ((ulong)entryCount + entriesPerSector - 1) / entriesPerSector;
        if (entryStartLba + arraySectors > device.BlockCount)
        {
            return false;
        }

        layout = new EntryArrayLayout(entryStartLba, entryCount, entrySize, entriesPerSector, arraySectors);
        return true;
    }

    /// <summary>
    /// Whether [<paramref name="startSector"/>, +<paramref name="sectorCount"/>)
    /// intersects a partition other than the one at
    /// <paramref name="excludeIndex"/> in <see cref="Parse"/>'s index space,
    /// or any partition at all when the index is negative. Every writer in
    /// this class asks this before stamping an entry, and
    /// <see cref="PartitionManager.MoveWithData"/> asks it before copying
    /// data so a refused move never touches the disk. A disk without a valid
    /// header has no entries to overlap. The range must already be bounded
    /// against the device.
    /// </summary>
    internal static bool OverlapsOtherEntry(IBlockDevice device, int excludeIndex, ulong startSector, ulong sectorCount)
    {
        return TryReadEntryArrayLayout(device, out EntryArrayLayout layout)
            && OverlapsOtherEntry(device, layout, excludeIndex, startSector, sectorCount);
    }

    private static bool OverlapsOtherEntry(IBlockDevice device, EntryArrayLayout layout, int excludeIndex, ulong startSector, ulong sectorCount)
    {
        (ulong entryStartLba, uint entryCount, uint entrySize, uint entriesPerSector, ulong arraySectors) = layout;
        int seen = 0;
        Span<byte> sector = new byte[device.BlockSize];
        for (uint s = 0; s < entryCount; s += entriesPerSector)
        {
            device.ReadBlock(entryStartLba + s / entriesPerSector, 1, sector);
            uint thisSector = (uint)Math.Min((ulong)entriesPerSector, entryCount - s);
            for (uint j = 0; j < thisSector; j++)
            {
                int offset = (int)(j * entrySize);
                if (IsZero(sector.Slice(offset, GuidFieldSize)))
                {
                    continue;
                }

                // Count only the entries Parse reports, so excludeIndex
                // names the same partition here as it does in MutateEntry.
                ulong otherStart = BitConverter.ToUInt64(sector.Slice(offset + EntryFirstLbaOffset, UInt64FieldSize));
                ulong otherEnd = BitConverter.ToUInt64(sector.Slice(offset + EntryLastLbaOffset, UInt64FieldSize));
                if (otherEnd < otherStart || otherStart < entryStartLba + arraySectors || otherEnd >= device.BlockCount)
                {
                    continue;
                }

                int thisIndex = seen;
                seen++;
                if (thisIndex == excludeIndex)
                {
                    continue;
                }

                // otherEnd is inclusive.
                if (startSector <= otherEnd && otherStart < startSector + sectorCount)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Locate the <paramref name="index"/>-th non-empty entry in the
    /// partition entry array, apply <paramref name="mutator"/> to it, and
    /// write the containing sector back. Returns false when the header is
    /// missing/corrupt, the index does not resolve to a used slot, or the
    /// mutator aborts. Applies the same distrust of on-disk header fields
    /// as <see cref="Parse"/> — CRCs are 0, so every field is range-checked
    /// before it drives I/O.
    /// </summary>
    private static bool MutateEntry(IBlockDevice device, int index, EntryMutator mutator)
    {
        if (index < 0 || !TryReadEntryArrayLayout(device, out EntryArrayLayout layout))
        {
            return false;
        }

        (ulong entryStartLba, uint entryCount, uint entrySize, uint entriesPerSector, ulong arraySectors) = layout;
        ulong blockSize = device.BlockSize;

        int seen = 0;
        Span<byte> sector = new byte[blockSize];
        for (uint s = 0; s < entryCount; s += entriesPerSector)
        {
            ulong lba = entryStartLba + s / entriesPerSector;
            device.ReadBlock(lba, 1, sector);
            uint thisSector = (uint)Math.Min((ulong)entriesPerSector, entryCount - s);
            for (uint j = 0; j < thisSector; j++)
            {
                int offset = (int)(j * entrySize);
                if (IsZero(sector.Slice(offset, GuidFieldSize)))
                {
                    continue;
                }

                // Skip entries with exactly Parse's validity criteria so
                // index stays aligned with Parse's output — one
                // corrupt entry ahead of the target would otherwise shift
                // every later index onto a different, healthy partition.
                // This also guarantees mutators only see validated LBAs
                // (startLba < BlockCount), so ResizePartition's
                // BlockCount - startLba arithmetic cannot underflow.
                ulong entryStart = BitConverter.ToUInt64(sector.Slice(offset + EntryFirstLbaOffset, UInt64FieldSize));
                ulong entryEnd = BitConverter.ToUInt64(sector.Slice(offset + EntryLastLbaOffset, UInt64FieldSize));
                if (entryEnd < entryStart
                    || entryStart < entryStartLba + arraySectors
                    || entryEnd >= device.BlockCount)
                {
                    continue;
                }

                if (seen == index)
                {
                    if (!mutator(sector.Slice(offset, (int)entrySize), layout))
                    {
                        return false;
                    }
                    device.WriteBlock(lba, 1, sector);
                    return true;
                }
                seen++;
            }
        }

        return false;
    }

    private static Guid ReadGuid(Span<byte> source)
    {
        byte[] bytes = new byte[GuidFieldSize];
        source.Slice(0, GuidFieldSize).CopyTo(bytes);
        return new Guid(bytes);
    }

    private static void WriteGuid(Span<byte> dest, Guid value)
    {
        byte[] bytes = value.ToByteArray();
        bytes.AsSpan().CopyTo(dest);
    }

    private static void WriteDeterministicGuid(Span<byte> dest, ulong mix)
    {
        BitConverter.TryWriteBytes(dest.Slice(GuidDword0Offset, UInt32FieldSize), (uint)mix);
        BitConverter.TryWriteBytes(dest.Slice(GuidDword1Offset, UInt32FieldSize), (uint)(mix ^ GuidMixSalt1));
        BitConverter.TryWriteBytes(dest.Slice(GuidDword2Offset, UInt32FieldSize), (uint)(mix ^ GuidMixSalt2));
        BitConverter.TryWriteBytes(dest.Slice(GuidDword3Offset, UInt32FieldSize), (uint)(mix ^ GuidMixSalt3));
    }

    private static bool IsZero(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != 0)
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>Single parsed partition. Sector positions are absolute on the host disk.</summary>
public sealed class GptPartitionEntry
{
    /// <summary>Partition type GUID (see <see cref="Gpt.BasicDataPartitionType"/>).</summary>
    public Guid PartitionType { get; }

    /// <summary>Unique partition GUID.</summary>
    public Guid PartitionGuid { get; }

    /// <summary>First absolute LBA of the partition on the host disk.</summary>
    public ulong StartSector { get; }

    /// <summary>Length of the partition in sectors.</summary>
    public ulong SectorCount { get; }

    /// <summary>Creates a GPT partition entry.</summary>
    public GptPartitionEntry(Guid partitionType, Guid partitionGuid, ulong startSector, ulong sectorCount)
    {
        PartitionType = partitionType;
        PartitionGuid = partitionGuid;
        StartSector = startSector;
        SectorCount = sectorCount;
    }
}
