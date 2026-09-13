// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// MBR (Master Boot Record) partition table parser / writer. Operates on
/// any <see cref="IBlockDevice"/> with 512-byte sectors.
/// </summary>
public static class Mbr
{
    /// <summary>Boot signature value stamped at bytes 510-511 of MBR and EBR sectors.</summary>
    internal const ushort MbrSignature = 0xAA55;

    /// <summary>Byte offset of the four-slot partition table within the MBR sector.</summary>
    internal const int PartitionTableOffset = 446;

    /// <summary>Size in bytes of one partition table entry.</summary>
    internal const int PartitionEntrySize = 16;

    /// <summary>Byte offset of the 0xAA55 boot signature within the MBR sector (bytes 510-511).</summary>
    internal const int SignatureOffset = 510;

    /// <summary>Size in bytes of the 0xAA55 boot signature field.</summary>
    internal const int SignatureSizeBytes = 2;

    /// <summary>Number of primary partition slots in an MBR partition table.</summary>
    internal const int MaxPartitions = 4;

    /// <summary>Byte offset of the status (boot indicator) byte within a partition entry.</summary>
    internal const int EntryStatusOffset = 0;

    /// <summary>Byte offset of the starting CHS head within a partition entry.</summary>
    internal const int EntryStartChsHeadOffset = 1;

    /// <summary>Byte offset of the starting CHS sector within a partition entry.</summary>
    internal const int EntryStartChsSectorOffset = 2;

    /// <summary>Byte offset of the starting CHS cylinder within a partition entry.</summary>
    internal const int EntryStartChsCylinderOffset = 3;

    /// <summary>Byte offset of the partition type (system ID) within a partition entry.</summary>
    internal const int EntrySystemIdOffset = 4;

    /// <summary>Byte offset of the ending CHS head within a partition entry.</summary>
    internal const int EntryEndChsHeadOffset = 5;

    /// <summary>Byte offset of the ending CHS sector within a partition entry.</summary>
    internal const int EntryEndChsSectorOffset = 6;

    /// <summary>Byte offset of the ending CHS cylinder within a partition entry.</summary>
    internal const int EntryEndChsCylinderOffset = 7;

    /// <summary>Byte offset of the 32-bit starting LBA within a partition entry.</summary>
    internal const int EntryStartLbaOffset = 8;

    /// <summary>Byte offset of the 32-bit sector count within a partition entry.</summary>
    internal const int EntrySectorCountOffset = 12;

    /// <summary>Size in bytes of the 32-bit LBA / sector-count fields in a partition entry.</summary>
    internal const int LbaFieldSizeBytes = 4;

    /// <summary>Largest value the 32-bit LBA / sector-count fields in a partition entry can hold; larger geometry is clamped or rejected by writers.</summary>
    internal const uint LbaFieldMaxValue = uint.MaxValue;

    /// <summary>CHS field placeholder stamped when the geometry exceeds CHS addressing (all bits set).</summary>
    internal const byte ChsPlaceholder = 0xFF;

    /// <summary>LBA of the MBR sector itself (sector 0) — where the partition table is read/written, and the start LBA no partition entry may alias.</summary>
    internal const uint MbrSectorLba = 0;

    /// <summary>Partition status byte: inactive (non-bootable) entry; also the boot indicator <see cref="Gpt"/> stamps on the protective entry (UEFI spec 5.2.3).</summary>
    internal const byte StatusInactive = 0x00;

    /// <summary>Partition status byte: active (bootable) entry.</summary>
    private const byte StatusBootable = 0x80;

    /// <summary>System ID 0x00 - unused (empty) partition table slot.</summary>
    internal const byte SystemIdEmpty = 0x00;

    /// <summary>System ID 0x05 - extended partition (CHS-addressed EBR container).</summary>
    internal const byte SystemIdExtendedChs = 0x05;

    /// <summary>System ID 0x0F - extended partition (LBA-addressed EBR container).</summary>
    internal const byte SystemIdExtendedLba = 0x0F;

    /// <summary>System ID 0x85 - Linux extended partition (EBR container).</summary>
    internal const byte SystemIdLinuxExtended = 0x85;

    /// <summary>System ID 0xEE - GPT protective/hybrid MBR entry (the real table is the GPT).</summary>
    internal const byte SystemIdGptProtective = 0xEE;

    /// <summary>True if LBA 0 ends with the 0xAA55 MBR signature.</summary>
    public static bool IsMbr(IBlockDevice device)
    {
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);
        return BitConverter.ToUInt16(mbr.Slice(SignatureOffset, SignatureSizeBytes)) == MbrSignature;
    }

    /// <summary>
    /// Parse the MBR's four primary partition entries. Empty (SystemId=0)
    /// entries are skipped. Extended (0x05/0x0F/0x85) entries are skipped
    /// here; use <see cref="TryGetExtendedPartition(IBlockDevice, out ulong)"/>
    /// to walk logicals via <see cref="Ebr"/>.
    /// </summary>
    public static List<MbrPartitionEntry> Parse(IBlockDevice device)
    {
        List<MbrPartitionEntry> partitions = new(MaxPartitions);
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        for (int i = 0; i < MaxPartitions; i++)
        {
            int offset = PartitionTableOffset + i * PartitionEntrySize;
            byte status = mbr[offset];
            byte systemId = mbr[offset + EntrySystemIdOffset];
            // Skip empty and extended (EBR) entries, and 0xEE protective/
            // hybrid GPT entries — the real table is the GPT; registering
            // the protective entry as a data partition (e.g. when the
            // primary GPT header is damaged) would let a write destroy the
            // remaining GPT structures.
            if (systemId == SystemIdEmpty || systemId == SystemIdExtendedChs || systemId == SystemIdExtendedLba || systemId == SystemIdLinuxExtended || systemId == SystemIdGptProtective)
            {
                continue;
            }
            // The status byte must be 0x00 (inactive) or 0x80 (bootable);
            // anything else marks a corrupt entry.
            if (status != StatusInactive && status != StatusBootable)
            {
                continue;
            }

            ulong startSector = BitConverter.ToUInt32(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes));
            ulong sectorCount = BitConverter.ToUInt32(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes));
            // Distrust on-disk metadata, mirroring the GPT hardening: start 0
            // aliases the MBR sector itself (a write through that "partition"
            // destroys the table), and a range past the disk end would
            // authorize wild host I/O through the resulting Partition.
            if (startSector == MbrSectorLba || sectorCount == 0 || startSector + sectorCount > device.BlockCount)
            {
                continue;
            }
            partitions.Add(new MbrPartitionEntry(systemId, startSector, sectorCount));
        }

        return partitions;
    }

    /// <summary>
    /// Write a fresh, empty-but-signed MBR to LBA 0. No boot code, all
    /// four partition slots zeroed; callers add entries via
    /// <see cref="AddPartition"/>. Also wipes LBA 1 (wipefs-style):
    /// Create claims the whole disk label, and a stale "EFI PART" header
    /// left there from a previous GPT layout would keep winning over this
    /// MBR in the partition scanner, which checks GPT first.
    /// </summary>
    public static void Create(IBlockDevice device)
    {
        Span<byte> mbr = new byte[device.BlockSize];
        BitConverter.TryWriteBytes(mbr.Slice(SignatureOffset, SignatureSizeBytes), MbrSignature);
        device.WriteBlock(MbrSectorLba, 1, mbr);

        // Wipe the primary GPT header sector: a stale "EFI PART" label from
        // a previous GPT layout would keep outranking this MBR in the
        // partition scanner, which checks GPT first.
        Span<byte> lba1 = new byte[device.BlockSize];
        device.WriteBlock(Gpt.PrimaryHeaderLba, 1, lba1);
    }

    /// <summary>
    /// Stamp partition <paramref name="index"/> (0..3) with a 32-bit
    /// LBA-addressed entry pointing at <paramref name="startSector"/> /
    /// <paramref name="sectorCount"/> with the given <paramref name="systemId"/>.
    /// </summary>
    /// <returns>
    /// <see langword="false"/>, writing nothing, when the geometry is one
    /// <see cref="Parse"/> would drop or the range overlaps another primary.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0..3.</exception>
    public static bool AddPartition(IBlockDevice device, int index, byte systemId, ulong startSector, ulong sectorCount)
    {
        if ((uint)index >= MaxPartitions)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "MBR primary partition index must be 0..3.");
        }
        // The on-disk LBA fields are 32-bit, so anything larger would be
        // truncated by the casts below; refuse it as Ebr.TryAddLogical does
        // rather than stamping a range the caller did not ask for.
        if (startSector > LbaFieldMaxValue || sectorCount > LbaFieldMaxValue)
        {
            return false;
        }
        // Same rules Parse enforces on read: start 0 aliases the MBR sector
        // itself, and a range past the disk end would authorize wild host
        // I/O through the resulting Partition. Reject at write time rather
        // than stamping an entry our own parser will drop.
        if (startSector == MbrSectorLba || sectorCount == 0)
        {
            return false;
        }
        // Both operands are bounded by LbaFieldMaxValue above, so the sum
        // cannot wrap.
        if (startSector + sectorCount > device.BlockCount)
        {
            return false;
        }

        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        // ResizePartition and MovePartition both refuse a range that walks into
        // another primary; a fresh entry must too, or the two alias the same
        // sectors and a write through one corrupts the other.
        if (OverlapsOtherPrimary(mbr, index, startSector, sectorCount))
        {
            return false;
        }

        int offset = PartitionTableOffset + index * PartitionEntrySize;
        mbr.Slice(offset, PartitionEntrySize).Clear();
        mbr[offset + EntrySystemIdOffset] = systemId;
        BitConverter.TryWriteBytes(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes), (uint)startSector);
        BitConverter.TryWriteBytes(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes), (uint)sectorCount);

        BitConverter.TryWriteBytes(mbr.Slice(SignatureOffset, SignatureSizeBytes), MbrSignature);
        device.WriteBlock(MbrSectorLba, 1, mbr);
        return true;
    }

    /// <summary>
    /// Locate the first extended partition (system ID 0x05, 0x0F, or 0x85)
    /// in the MBR's primary table and return its absolute start LBA.
    /// </summary>
    public static bool TryGetExtendedPartition(IBlockDevice device, out ulong startSector)
    {
        return TryGetExtendedPartition(device, out startSector, out _);
    }

    /// <summary>
    /// As <see cref="TryGetExtendedPartition(IBlockDevice, out ulong)"/> but
    /// also returns the extended partition's sector count.
    /// </summary>
    public static bool TryGetExtendedPartition(IBlockDevice device, out ulong startSector, out ulong sectorCount)
    {
        startSector = 0;
        sectorCount = 0;
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        for (int i = 0; i < MaxPartitions; i++)
        {
            int offset = PartitionTableOffset + i * PartitionEntrySize;
            byte systemId = mbr[offset + EntrySystemIdOffset];
            if (systemId == SystemIdExtendedChs || systemId == SystemIdExtendedLba || systemId == SystemIdLinuxExtended)
            {
                ulong start = BitConverter.ToUInt32(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes));
                ulong count = BitConverter.ToUInt32(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes));
                // Same distrust of on-disk metadata as Parse: start 0
                // aliases the MBR sector itself, and a range past the disk
                // end flows into EBR sector I/O. Skip the corrupt slot so
                // a valid extended entry behind it is still found.
                if (start == MbrSectorLba || count == 0 || start + count > device.BlockCount)
                {
                    continue;
                }
                startSector = start;
                sectorCount = count;
                return true;
            }
        }

        return false;
    }

    /// <summary>Mark primary slot <paramref name="index"/> as empty by zeroing its 16-byte entry.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0..3.</exception>
    public static void RemovePartition(IBlockDevice device, int index)
    {
        if ((uint)index >= MaxPartitions)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "MBR primary partition index must be 0..3.");
        }

        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        int offset = PartitionTableOffset + index * PartitionEntrySize;
        mbr.Slice(offset, PartitionEntrySize).Clear();

        BitConverter.TryWriteBytes(mbr.Slice(SignatureOffset, SignatureSizeBytes), MbrSignature);
        device.WriteBlock(MbrSectorLba, 1, mbr);
    }

    /// <summary>Rewrite the SectorCount field of slot <paramref name="index"/>, leaving systemId / startSector untouched.</summary>
    /// <returns>
    /// <see langword="false"/>, writing nothing, when the slot is not a data
    /// partition, or when the new range is empty, runs past the device, or
    /// overlaps another primary entry.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0..3.</exception>
    public static bool ResizePartition(IBlockDevice device, int index, ulong newSectorCount)
    {
        if ((uint)index >= MaxPartitions)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "MBR primary partition index must be 0..3.");
        }
        if (newSectorCount > LbaFieldMaxValue)
        {
            return false;
        }

        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        int offset = PartitionTableOffset + index * PartitionEntrySize;
        if (!IsMutableSystemId(mbr[offset + EntrySystemIdOffset]))
        {
            return false;
        }

        // Same write-time rejection as AddPartition: never stamp a
        // geometry our own parser will drop.
        ulong startSector = BitConverter.ToUInt32(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes));
        if (newSectorCount == 0 || startSector + newSectorCount > device.BlockCount)
        {
            return false;
        }
        if (OverlapsOtherPrimary(mbr, index, startSector, newSectorCount))
        {
            return false;
        }

        BitConverter.TryWriteBytes(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes), (uint)newSectorCount);
        BitConverter.TryWriteBytes(mbr.Slice(SignatureOffset, SignatureSizeBytes), MbrSignature);
        device.WriteBlock(MbrSectorLba, 1, mbr);
        return true;
    }

    /// <summary>Rewrite the StartSector field of slot <paramref name="index"/>, leaving systemId / sectorCount untouched. Table-level only — does not relocate data.</summary>
    /// <returns>
    /// <see langword="false"/>, writing nothing, when the slot is not a data
    /// partition, or when the new range starts at LBA 0, runs past the
    /// device, or overlaps another primary entry.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not 0..3.</exception>
    public static bool MovePartition(IBlockDevice device, int index, ulong newStartSector)
    {
        if ((uint)index >= MaxPartitions)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "MBR primary partition index must be 0..3.");
        }
        if (newStartSector > LbaFieldMaxValue)
        {
            return false;
        }

        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);

        int offset = PartitionTableOffset + index * PartitionEntrySize;
        if (!IsMutableSystemId(mbr[offset + EntrySystemIdOffset]))
        {
            return false;
        }

        // Same write-time rejection as AddPartition: start 0 aliases the
        // MBR sector itself, and a range past the disk end would authorize
        // wild host I/O through the resulting Partition.
        ulong sectorCount = BitConverter.ToUInt32(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes));
        if (newStartSector == MbrSectorLba || newStartSector + sectorCount > device.BlockCount)
        {
            return false;
        }
        if (OverlapsOtherPrimary(mbr, index, newStartSector, sectorCount))
        {
            return false;
        }

        BitConverter.TryWriteBytes(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes), (uint)newStartSector);
        BitConverter.TryWriteBytes(mbr.Slice(SignatureOffset, SignatureSizeBytes), MbrSignature);
        device.WriteBlock(MbrSectorLba, 1, mbr);
        return true;
    }

    /// <summary>
    /// Guard for the entry-level mutators: empty slots have nothing to
    /// edit; extended containers (0x05/0x0F/0x85) are never surfaced by
    /// <see cref="Parse"/> and their EBR chain would be orphaned or cut
    /// off by a table-level resize/move (use the <see cref="Ebr"/> APIs
    /// for logical-volume management); the 0xEE protective entry guards
    /// the GPT structures.
    /// </summary>
    /// <remarks>
    /// Internal so <see cref="PartitionManager"/> can ask the same question
    /// before it acts: it resolves a slot from the raw table, which matches
    /// the entries this refuses, and <c>MoveWithData</c> must decline before
    /// it copies rather than after.
    /// </remarks>
    internal static bool IsMutableSystemId(byte systemId)
    {
        return systemId != SystemIdEmpty
            && systemId != SystemIdExtendedChs
            && systemId != SystemIdExtendedLba
            && systemId != SystemIdLinuxExtended
            && systemId != SystemIdGptProtective;
    }

    /// <summary>
    /// Whether [<paramref name="startSector"/>, +<paramref name="sectorCount"/>)
    /// intersects a primary entry other than slot <paramref name="excludeIndex"/>,
    /// or any primary when the slot is negative. The extended container
    /// counts as occupied, since its logicals live inside it. Every writer
    /// in this class asks this before stamping an entry, and
    /// <see cref="PartitionManager.MoveWithData"/> asks it before copying
    /// data so a refused move never touches the disk.
    /// </summary>
    internal static bool OverlapsOtherPrimary(IBlockDevice device, int excludeIndex, ulong startSector, ulong sectorCount)
    {
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(MbrSectorLba, 1, mbr);
        return OverlapsOtherPrimary(mbr, excludeIndex, startSector, sectorCount);
    }

    /// <summary>
    /// True when [<paramref name="startSector"/>, +<paramref name="sectorCount"/>)
    /// intersects any occupied primary slot other than <paramref name="index"/>.
    /// Slots with geometry <see cref="Parse"/> would drop are skipped.
    /// </summary>
    private static bool OverlapsOtherPrimary(Span<byte> mbr, int index, ulong startSector, ulong sectorCount)
    {
        for (int i = 0; i < MaxPartitions; i++)
        {
            if (i == index)
            {
                continue;
            }
            int offset = PartitionTableOffset + i * PartitionEntrySize;
            if (mbr[offset + EntrySystemIdOffset] == SystemIdEmpty)
            {
                continue;
            }
            ulong otherStart = BitConverter.ToUInt32(mbr.Slice(offset + EntryStartLbaOffset, LbaFieldSizeBytes));
            ulong otherCount = BitConverter.ToUInt32(mbr.Slice(offset + EntrySectorCountOffset, LbaFieldSizeBytes));
            if (otherStart == MbrSectorLba || otherCount == 0)
            {
                continue;
            }
            if (startSector < otherStart + otherCount && otherStart < startSector + sectorCount)
            {
                return true;
            }
        }
        return false;
    }
}

/// <summary>Single MBR partition entry. Sector positions are absolute on the host disk.</summary>
public sealed class MbrPartitionEntry
{
    /// <summary>MBR partition type byte (e.g. 0x83 Linux, 0x0B FAT32).</summary>
    public byte SystemId { get; }

    /// <summary>First absolute LBA of the partition on the host disk.</summary>
    public ulong StartSector { get; }

    /// <summary>Length of the partition in sectors.</summary>
    public ulong SectorCount { get; }

    /// <summary>Creates an MBR partition entry.</summary>
    public MbrPartitionEntry(byte systemId, ulong startSector, ulong sectorCount)
    {
        SystemId = systemId;
        StartSector = startSector;
        SectorCount = sectorCount;
    }
}
