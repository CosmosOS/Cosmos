// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// High-level partition lifecycle (create / delete / resize / move) on top
/// of <see cref="Mbr"/> and <see cref="Gpt"/>. Auto-detects the table type
/// per call. <see cref="MoveWithData"/> physically copies sectors before
/// rewriting the table; the other operations are pure table edits.
/// </summary>
public static class PartitionManager
{
    /// <summary>Sectors copied per ReadBlock/WriteBlock batch in <see cref="CopySectors"/>.</summary>
    private const int BatchBlocks = 128;

    /// <summary>Identifies a partition by its absolute LBA range on the host disk.</summary>
    public readonly struct PartitionLocation
    {
        /// <summary>Absolute LBA on the host disk where the partition begins.</summary>
        public ulong StartSector { get; }

        /// <summary>Length of the partition in sectors.</summary>
        public ulong SectorCount { get; }

        /// <summary>
        /// Creates a location from an absolute LBA range.
        /// </summary>
        /// <param name="startSector">Absolute LBA on the host disk where the partition begins.</param>
        /// <param name="sectorCount">Length of the partition in sectors.</param>
        public PartitionLocation(ulong startSector, ulong sectorCount)
        {
            StartSector = startSector;
            SectorCount = sectorCount;
        }
    }

    /// <summary>
    /// Add a partition. On a GPT disk uses <paramref name="gptType"/>; on an
    /// MBR disk picks the first free primary slot and uses
    /// <paramref name="mbrSystemId"/>. Returns false if no table is present
    /// or no slot is free.
    /// </summary>
    public static bool Create(
        IBlockDevice device,
        ulong startSector,
        ulong sectorCount,
        byte mbrSystemId,
        Guid gptType)
    {
        // Start 0 aliases the table sector on both formats, so both writers
        // refuse it; check here too, so the reason is stated once at the top.
        if (sectorCount == 0 || startSector == 0)
        {
            return false;
        }
        // Non-wrapping bound (cf. Gpt.AddPartition): the naive sum wraps
        // 2^64 for large inputs and slips past the check.
        if (startSector >= device.BlockCount || sectorCount > device.BlockCount - startSector)
        {
            return false;
        }
        // Each writer refuses a range that overlaps another entry of its own
        // table (Gpt.AddPartition, Mbr.AddPartition), so the facade only
        // routes to the right one.

        if (Gpt.IsGpt(device))
        {
            return Gpt.AddPartition(device, startSector, sectorCount, gptType);
        }

        if (!Mbr.IsMbr(device))
        {
            return false;
        }

        int freeSlot = FindFreeMbrSlot(device);
        if (freeSlot < 0)
        {
            return false;
        }

        // AddPartition owns the 32-bit on-disk field bound and reports it the
        // same way this does, so the facade no longer pre-checks it.
        return Mbr.AddPartition(device, freeSlot, mbrSystemId, startSector, sectorCount);
    }

    /// <summary>
    /// Add a logical partition to the disk's extended partition.
    /// </summary>
    /// <param name="device">The disk to add the logical partition to.</param>
    /// <param name="systemId">Partition type byte to stamp on the new logical.</param>
    /// <param name="sectorCount">Length of the new logical in sectors.</param>
    /// <param name="startSector">Absolute LBA the new logical begins at, when the call succeeds.</param>
    /// <returns>
    /// <see langword="false"/> when the disk is GPT or unpartitioned, has no
    /// extended container, or the container has no room left.
    /// </returns>
    public static bool TryCreateLogical(IBlockDevice device, byte systemId, ulong sectorCount, out ulong startSector)
    {
        startSector = 0;

        if (Gpt.IsGpt(device))
        {
            return false;
        }
        if (!Mbr.IsMbr(device))
        {
            return false;
        }
        if (!Mbr.TryGetExtendedPartition(device, out ulong extStart, out ulong extCount))
        {
            return false;
        }
        return Ebr.TryAddLogical(device, extStart, extCount, systemId, sectorCount, out startSector);
    }

    /// <summary>Delete the partition occupying <paramref name="location"/>.</summary>
    public static bool Delete(IBlockDevice device, PartitionLocation location)
    {
        if (Gpt.IsGpt(device))
        {
            int gptIndex = FindGptIndex(device, location);
            if (gptIndex < 0)
            {
                return false;
            }
            return Gpt.RemovePartition(device, gptIndex);
        }

        if (!Mbr.IsMbr(device))
        {
            return false;
        }

        if (TryFindLogical(device, location, out ulong extStart, out int logicalIndex))
        {
            return Ebr.RemoveLogical(device, extStart, logicalIndex);
        }

        int slot = FindMbrSlot(device, location);
        if (slot < 0)
        {
            return false;
        }
        return Mbr.RemovePartition(device, slot);
    }

    /// <summary>Resize the partition at <paramref name="location"/> to <paramref name="newSectorCount"/>. Table-only; does not adjust the filesystem inside.</summary>
    public static bool Resize(IBlockDevice device, PartitionLocation location, ulong newSectorCount)
    {
        if (newSectorCount == 0)
        {
            return false;
        }
        // Non-wrapping bound: the naive sum wraps 2^64 for large counts.
        if (location.StartSector >= device.BlockCount
            || newSectorCount > device.BlockCount - location.StartSector)
        {
            return false;
        }
        // Growing into a neighbour is refused by each writer: Gpt.ResizePartition
        // and Mbr.ResizePartition test the other entries of their table, and
        // Ebr.ResizeLogical bounds the logical by the next EBR sector.

        if (Gpt.IsGpt(device))
        {
            int gptIndex = FindGptIndex(device, location);
            if (gptIndex < 0)
            {
                return false;
            }
            return Gpt.ResizePartition(device, gptIndex, newSectorCount);
        }

        if (!Mbr.IsMbr(device))
        {
            return false;
        }

        if (TryFindLogical(device, location, out ulong extStart, out int logicalIndex))
        {
            return Ebr.ResizeLogical(device, extStart, logicalIndex, newSectorCount);
        }

        int slot = FindMbrSlot(device, location, out byte systemId);
        if (slot < 0 || !Mbr.IsMutableSystemId(systemId))
        {
            return false;
        }
        return Mbr.ResizePartition(device, slot, newSectorCount);
    }

    /// <summary>
    /// Physically relocate the partition at <paramref name="location"/> to
    /// start at <paramref name="newStartSector"/>: copies all sectors first,
    /// then rewrites the table entry. Direction-aware to handle overlap.
    /// </summary>
    public static bool MoveWithData(IBlockDevice device, PartitionLocation location, ulong newStartSector)
    {
        if (location.SectorCount == 0)
        {
            return false;
        }
        // Non-wrapping bounds for both the source range (it drives raw
        // ReadBlock batches) and the destination: the naive sums wrap 2^64.
        if (location.StartSector >= device.BlockCount
            || location.SectorCount > device.BlockCount - location.StartSector)
        {
            return false;
        }
        if (newStartSector >= device.BlockCount
            || location.SectorCount > device.BlockCount - newStartSector)
        {
            return false;
        }
        if (newStartSector == location.StartSector)
        {
            return true;
        }
        // Both writers refuse a destination inside the table's own metadata
        // (Mbr.MovePartition rejects LBA 0, Gpt.MovePartition rejects anything
        // below the first usable LBA), but they run after the copy. Applying
        // the same rule here is what keeps a refused move side-effect free:
        // otherwise CopySectors overwrites the very table the writer is about
        // to re-read, and the call reports false having destroyed it.
        ulong firstPlaceableLba = Gpt.IsGpt(device) ? Gpt.FirstUsableLba : Mbr.MbrSectorLba + 1;
        if (newStartSector < firstPlaceableLba)
        {
            return false;
        }
        // The destination must be free space (the source itself may overlap
        // it; CopySectors is direction-aware). Each format's writer owns that
        // test and applies it again when it stamps the entry; asking it here,
        // once the entry is resolved and before the copy, is what keeps a
        // refused move side-effect free.

        // Resolve the table entry and its constraints BEFORE copying, so a
        // false return is side-effect free. The copy-then-retable order
        // stays (a crash between the two leaves the old table pointing at
        // intact data), and the Flush makes the copied data durable before
        // the table points at it.
        if (Gpt.IsGpt(device))
        {
            int gptIndex = FindGptIndex(device, location);
            if (gptIndex < 0 || Gpt.OverlapsOtherEntry(device, gptIndex, newStartSector, location.SectorCount))
            {
                return false;
            }
            CopySectors(device, location.StartSector, newStartSector, location.SectorCount);
            device.Flush();
            return Gpt.MovePartition(device, gptIndex, newStartSector);
        }

        if (!Mbr.IsMbr(device))
        {
            return false;
        }

        if (TryFindLogical(device, location, out ulong extStart, out int logicalIndex))
        {
            if (!Ebr.CanMoveLogical(device, extStart, logicalIndex, newStartSector))
            {
                return false;
            }
            CopySectors(device, location.StartSector, newStartSector, location.SectorCount);
            device.Flush();
            return Ebr.MoveLogical(device, extStart, logicalIndex, newStartSector);
        }

        // The mutability test belongs with the other pre-copy checks: this
        // walks the raw table, so it matches the extended container and the
        // GPT protective entry, which Mbr.MovePartition then refuses. Asking
        // afterwards meant the sectors were already copied and the refusal
        // arrived as an exception out of a bool-returning method.
        int slot = FindMbrSlot(device, location, out byte systemId);
        if (slot < 0 || !Mbr.IsMutableSystemId(systemId))
        {
            return false;
        }
        if (newStartSector > Mbr.LbaFieldMaxValue
            || Mbr.OverlapsOtherPrimary(device, slot, newStartSector, location.SectorCount))
        {
            return false;
        }

        CopySectors(device, location.StartSector, newStartSector, location.SectorCount);
        device.Flush();
        return Mbr.MovePartition(device, slot, newStartSector);
    }

    private static int FindFreeMbrSlot(IBlockDevice device)
    {
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(Mbr.MbrSectorLba, 1, mbr);
        for (int i = 0; i < Mbr.MaxPartitions; i++)
        {
            int offset = Mbr.PartitionTableOffset + i * Mbr.PartitionEntrySize;
            if (mbr[offset + Mbr.EntrySystemIdOffset] == Mbr.SystemIdEmpty)
            {
                return i;
            }
        }
        return -1;
    }

    private static int FindMbrSlot(IBlockDevice device, PartitionLocation location)
    {
        return FindMbrSlot(device, location, out _);
    }

    /// <summary>
    /// As <see cref="FindMbrSlot(IBlockDevice, PartitionLocation)"/>, also
    /// reporting the slot's system ID. This walks the raw table rather than
    /// <see cref="Mbr.Parse"/>, so it matches extended and protective entries
    /// too; callers that go on to mutate the slot must test
    /// <see cref="Mbr.IsMutableSystemId"/> before they act.
    /// </summary>
    private static int FindMbrSlot(IBlockDevice device, PartitionLocation location, out byte systemId)
    {
        systemId = Mbr.SystemIdEmpty;
        Span<byte> mbr = new byte[device.BlockSize];
        device.ReadBlock(Mbr.MbrSectorLba, 1, mbr);
        for (int i = 0; i < Mbr.MaxPartitions; i++)
        {
            int offset = Mbr.PartitionTableOffset + i * Mbr.PartitionEntrySize;
            byte slotSystemId = mbr[offset + Mbr.EntrySystemIdOffset];
            if (slotSystemId == Mbr.SystemIdEmpty)
            {
                continue;
            }
            ulong start = BitConverter.ToUInt32(mbr.Slice(offset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes));
            ulong count = BitConverter.ToUInt32(mbr.Slice(offset + Mbr.EntrySectorCountOffset, Mbr.LbaFieldSizeBytes));
            if (start == location.StartSector && count == location.SectorCount)
            {
                systemId = slotSystemId;
                return i;
            }
        }
        return -1;
    }

    private static bool TryFindLogical(IBlockDevice device, PartitionLocation location, out ulong extendedStartSector, out int logicalIndex)
    {
        extendedStartSector = 0;
        logicalIndex = -1;

        if (!Mbr.TryGetExtendedPartition(device, out ulong extStart, out ulong extCount))
        {
            return false;
        }
        if (location.StartSector < extStart || location.StartSector + location.SectorCount > extStart + extCount)
        {
            return false;
        }

        List<MbrPartitionEntry> logicals = Ebr.Parse(device, extStart);
        for (int i = 0; i < logicals.Count; i++)
        {
            if (logicals[i].StartSector == location.StartSector && logicals[i].SectorCount == location.SectorCount)
            {
                extendedStartSector = extStart;
                logicalIndex = i;
                return true;
            }
        }

        return false;
    }

    private static int FindGptIndex(IBlockDevice device, PartitionLocation location)
    {
        List<GptPartitionEntry> entries = Gpt.Parse(device);
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].StartSector == location.StartSector && entries[i].SectorCount == location.SectorCount)
            {
                return i;
            }
        }
        return -1;
    }

    private static void CopySectors(IBlockDevice device, ulong source, ulong destination, ulong count)
    {
        ulong blockSize = device.BlockSize;
        Span<byte> buffer = new byte[(int)blockSize * BatchBlocks];

        // Non-wrapping overlap test: source + count can wrap 2^64.
        bool overlapsForward = destination > source && destination - source < count;
        if (overlapsForward)
        {
            ulong remaining = count;
            while (remaining > 0)
            {
                ulong batch = remaining < BatchBlocks ? remaining : BatchBlocks;
                ulong tailOffset = remaining - batch;
                Span<byte> slice = buffer.Slice(0, (int)(batch * blockSize));
                device.ReadBlock(source + tailOffset, batch, slice);
                device.WriteBlock(destination + tailOffset, batch, slice);
                remaining -= batch;
            }
        }
        else
        {
            ulong copied = 0;
            while (copied < count)
            {
                ulong batch = count - copied < BatchBlocks ? count - copied : BatchBlocks;
                Span<byte> slice = buffer.Slice(0, (int)(batch * blockSize));
                device.ReadBlock(source + copied, batch, slice);
                device.WriteBlock(destination + copied, batch, slice);
                copied += batch;
            }
        }
    }
}
