// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// EBR (Extended Boot Record) chain parser and editor. An extended partition
/// contains a singly-linked list of EBR sectors; each EBR holds one logical
/// partition entry plus an optional pointer to the next EBR. Convention used:
/// <list type="bullet">
///   <item><description>Logical-entry start LBA is relative to the EBR sector that holds it.</description></item>
///   <item><description>Next-EBR pointer is relative to the extended partition's start LBA (the standard convention).</description></item>
///   <item><description>The first EBR always sits at the extended partition's start LBA.</description></item>
///   <item><description>Each subsequent EBR is placed immediately after the prior logical's data.</description></item>
/// </list>
/// </summary>
public static class Ebr
{
    /// <summary>Byte offset of this EBR's logical-partition entry (partition table slot 0).</summary>
    private const int LogicalEntryOffset = Mbr.PartitionTableOffset;

    /// <summary>Byte offset of the next-EBR link entry (partition table slot 1).</summary>
    private const int NextEbrEntryOffset = Mbr.PartitionTableOffset + Mbr.PartitionEntrySize;

    /// <summary>Sectors the EBR itself occupies; a logical's data area starts this many sectors after its hosting EBR (classic EBR layout). Internal so <see cref="PartitionManager"/>'s collision checks track the same placement.</summary>
    internal const uint EbrSectorSpan = 1;

    /// <summary>Defensive cap on chain hops so a cyclic on-disk chain cannot spin the walk forever.</summary>
    private const int MaxChainLength = 128;

    private struct ChainNode
    {
        public ulong EbrLba;
        public byte LogicalSystemId;
        public uint LogicalRelativeStart;
        public uint LogicalSectorCount;
        public uint NextRelative;
    }

    /// <summary>
    /// Walk the EBR chain rooted at <paramref name="extendedStartSector"/> and
    /// return one entry per logical partition. <c>StartSector</c> values are
    /// absolute LBAs on <paramref name="device"/>.
    /// </summary>
    public static List<MbrPartitionEntry> Parse(IBlockDevice device, ulong extendedStartSector)
    {
        List<MbrPartitionEntry> logicals = [];
        if (device is null)
        {
            return logicals;
        }

        List<ChainNode> chain = WalkChain(device, extendedStartSector);
        for (int i = 0; i < chain.Count; i++)
        {
            ChainNode node = chain[i];
            if (node.LogicalSystemId == Mbr.SystemIdEmpty)
            {
                continue;
            }
            logicals.Add(new MbrPartitionEntry(
                node.LogicalSystemId,
                node.EbrLba + node.LogicalRelativeStart,
                node.LogicalSectorCount));
        }
        return logicals;
    }

    /// <summary>
    /// Append a logical partition to the chain. The new EBR is placed right
    /// after the prior logical's data, and the new logical's data area
    /// follows the new EBR (one sector for the EBR, then <paramref name="sectorCount"/> sectors).
    /// </summary>
    /// <param name="device">The disk holding the extended container.</param>
    /// <param name="extendedStartSector">Absolute LBA where the extended container begins.</param>
    /// <param name="extendedSectorCount">Length of the extended container in sectors.</param>
    /// <param name="systemId">Partition type byte to stamp on the new logical.</param>
    /// <param name="sectorCount">Length of the new logical's data area in sectors.</param>
    /// <param name="startSector">Absolute LBA the new logical's data begins at, when the call succeeds.</param>
    /// <returns>
    /// <see langword="false"/>, writing nothing, when the geometry is one this
    /// file's own <see cref="Parse"/> would drop or there is no room left in
    /// the container.
    /// </returns>
    public static bool TryAddLogical(
        IBlockDevice device,
        ulong extendedStartSector,
        ulong extendedSectorCount,
        byte systemId,
        ulong sectorCount,
        out ulong startSector)
    {
        startSector = 0;

        // The on-disk field is 32-bit: a larger count would be silently
        // truncated by the (uint) cast below (2^32 stamps a zero-length
        // entry), so reject it up front as ResizeLogical does.
        if (sectorCount == 0 || sectorCount > uint.MaxValue
            || systemId == Mbr.SystemIdEmpty
            || systemId == Mbr.SystemIdExtendedChs
            || systemId == Mbr.SystemIdExtendedLba
            || systemId == Mbr.SystemIdLinuxExtended)
        {
            return false;
        }

        // The envelope is caller-supplied on-disk metadata (the MBR's
        // extended entry): clamp it to the device end so a corrupt count
        // can never authorize stamping a logical past the disk.
        if (extendedStartSector >= device.BlockCount)
        {
            return false;
        }
        ulong extendedEnd = extendedSectorCount > device.BlockCount - extendedStartSector
            ? device.BlockCount
            : extendedStartSector + extendedSectorCount;
        List<ChainNode> chain = WalkChain(device, extendedStartSector);

        ulong newEbrLba;
        if (chain.Count == 0)
        {
            newEbrLba = extendedStartSector;
        }
        else
        {
            ChainNode tail = chain[^1];
            newEbrLba = tail.EbrLba + tail.LogicalRelativeStart + tail.LogicalSectorCount;
        }

        if (newEbrLba + EbrSectorSpan + sectorCount > extendedEnd)
        {
            return false;
        }

        WriteEbrSector(device, newEbrLba, systemId, relativeStart: EbrSectorSpan, sectorCount: (uint)sectorCount, nextRelative: 0);

        if (chain.Count > 0)
        {
            ChainNode tail = chain[^1];
            uint newNextRelative = (uint)(newEbrLba - extendedStartSector);
            WriteEbrSector(
                device,
                tail.EbrLba,
                tail.LogicalSystemId,
                tail.LogicalRelativeStart,
                tail.LogicalSectorCount,
                newNextRelative);
        }

        startSector = newEbrLba + EbrSectorSpan;
        return true;
    }

    /// <summary>
    /// Remove the <paramref name="logicalIndex"/>-th logical partition from
    /// the chain (0-based, in chain order).
    /// </summary>
    public static bool RemoveLogical(IBlockDevice device, ulong extendedStartSector, int logicalIndex)
    {
        List<ChainNode> chain = WalkChain(device, extendedStartSector);
        if (logicalIndex < 0 || logicalIndex >= chain.Count)
        {
            return false;
        }

        if (logicalIndex == 0)
        {
            if (chain.Count == 1)
            {
                Span<byte> wipe = new byte[device.BlockSize];
                device.WriteBlock(extendedStartSector, 1, wipe);
                return true;
            }

            // Promote node[1] into the fixed first EBR slot at extendedStartSector.
            // LogicalRelativeStart is relative to the EBR sector holding the
            // entry, so it must be rebased from successor.EbrLba to the first
            // EBR; NextRelative is already extended-relative and stays as-is.
            ChainNode successor = chain[1];
            uint promotedRelativeStart =
                (uint)(successor.EbrLba + successor.LogicalRelativeStart - extendedStartSector);
            WriteEbrSector(
                device,
                extendedStartSector,
                successor.LogicalSystemId,
                promotedRelativeStart,
                successor.LogicalSectorCount,
                successor.NextRelative);
            return true;
        }

        // Bypass: predecessor's next pointer skips the deleted node.
        ChainNode predecessor = chain[logicalIndex - 1];
        ChainNode target = chain[logicalIndex];
        WriteEbrSector(
            device,
            predecessor.EbrLba,
            predecessor.LogicalSystemId,
            predecessor.LogicalRelativeStart,
            predecessor.LogicalSectorCount,
            target.NextRelative);
        return true;
    }

    /// <summary>Rewrite the SectorCount of the <paramref name="logicalIndex"/>-th logical partition.</summary>
    public static bool ResizeLogical(
        IBlockDevice device,
        ulong extendedStartSector,
        int logicalIndex,
        ulong newSectorCount)
    {
        if (newSectorCount == 0 || newSectorCount > uint.MaxValue)
        {
            return false;
        }

        List<ChainNode> chain = WalkChain(device, extendedStartSector);
        if (logicalIndex < 0 || logicalIndex >= chain.Count)
        {
            return false;
        }

        ChainNode node = chain[logicalIndex];
        ulong absoluteEnd = node.EbrLba + node.LogicalRelativeStart + newSectorCount;
        ulong upperBound = logicalIndex + 1 < chain.Count
            ? chain[logicalIndex + 1].EbrLba
            : extendedStartSector + ResolveExtendedCount(device, extendedStartSector);
        if (absoluteEnd > upperBound)
        {
            return false;
        }

        WriteEbrSector(
            device,
            node.EbrLba,
            node.LogicalSystemId,
            node.LogicalRelativeStart,
            (uint)newSectorCount,
            node.NextRelative);
        return true;
    }

    /// <summary>
    /// Rewrite the start of the <paramref name="logicalIndex"/>-th logical
    /// partition so its data begins at the absolute host LBA
    /// <paramref name="newStartSector"/>. The hosting EBR sector stays put;
    /// only the relative offset inside the EBR changes. Caller is responsible for making sure data
    /// at the new range is what's expected (use
    /// <see cref="PartitionManager.MoveWithData"/> for a data-copying move).
    /// </summary>
    public static bool MoveLogical(
        IBlockDevice device,
        ulong extendedStartSector,
        int logicalIndex,
        ulong newStartSector)
    {
        if (!TryPlanMove(device, extendedStartSector, logicalIndex, newStartSector, out ChainNode node, out uint newRelative))
        {
            return false;
        }

        WriteEbrSector(
            device,
            node.EbrLba,
            node.LogicalSystemId,
            newRelative,
            node.LogicalSectorCount,
            node.NextRelative);
        return true;
    }

    /// <summary>
    /// Whether the <paramref name="logicalIndex"/>-th logical partition may
    /// have its data start at <paramref name="newStartSector"/>: past its
    /// own EBR sector, and ending no later than the next logical's EBR
    /// sector or the container's end. <see cref="MoveLogical"/> applies
    /// exactly this test when it stamps the entry, and
    /// <see cref="PartitionManager.MoveWithData"/> asks it before copying
    /// data so a refused move never touches the disk.
    /// </summary>
    internal static bool CanMoveLogical(IBlockDevice device, ulong extendedStartSector, int logicalIndex, ulong newStartSector) =>
        TryPlanMove(device, extendedStartSector, logicalIndex, newStartSector, out _, out _);

    /// <summary>
    /// Resolve a logical move: walk the chain to the
    /// <paramref name="logicalIndex"/>-th node, and accept
    /// <paramref name="newStartSector"/> only when it lies past that node's
    /// own EBR sector, fits in the EBR's 32-bit relative field, and ends no
    /// later than the next node's EBR sector or the container's end. On
    /// success, hands back the node and the relative start to stamp.
    /// </summary>
    private static bool TryPlanMove(
        IBlockDevice device,
        ulong extendedStartSector,
        int logicalIndex,
        ulong newStartSector,
        out ChainNode node,
        out uint newRelative)
    {
        node = default;
        newRelative = 0;

        List<ChainNode> chain = WalkChain(device, extendedStartSector);
        if (logicalIndex < 0 || logicalIndex >= chain.Count)
        {
            return false;
        }

        node = chain[logicalIndex];
        if (newStartSector <= node.EbrLba)
        {
            return false;
        }

        ulong relative = newStartSector - node.EbrLba;
        if (relative > uint.MaxValue)
        {
            return false;
        }

        ulong newEndSector = newStartSector + node.LogicalSectorCount;
        ulong upperBound = logicalIndex + 1 < chain.Count
            ? chain[logicalIndex + 1].EbrLba
            : extendedStartSector + ResolveExtendedCount(device, extendedStartSector);
        if (newEndSector > upperBound)
        {
            return false;
        }

        newRelative = (uint)relative;
        return true;
    }

    private static List<ChainNode> WalkChain(IBlockDevice device, ulong extendedStartSector)
    {
        List<ChainNode> nodes = [];
        if (device is null)
        {
            return nodes;
        }

        // On-disk EBR metadata is untrusted, same rule as Mbr.Parse for
        // primaries: resolve the extended envelope once, drop logical
        // entries whose range leaves it, and stop the walk before a corrupt
        // next pointer sends ReadBlock outside it — past the device end
        // that read throws (AHCI surfaces a fatal command abort), and a
        // stray 0x55AA sector inside the disk (e.g. a FAT VBR) would parse
        // as garbage logicals.
        ulong envelopeEnd = extendedStartSector + ResolveExtendedCount(device, extendedStartSector);
        if (envelopeEnd > device.BlockCount)
        {
            envelopeEnd = device.BlockCount;
        }

        ulong currentEbrLba = extendedStartSector;
        int hops = 0;

        // ReadBlock fully overwrites the buffer each hop, so one
        // allocation serves the whole walk.
        Span<byte> sector = new byte[device.BlockSize];

        while (hops < MaxChainLength && currentEbrLba < envelopeEnd)
        {
            device.ReadBlock(currentEbrLba, 1, sector);

            if (BitConverter.ToUInt16(sector.Slice(Mbr.SignatureOffset, Mbr.SignatureSizeBytes)) != Mbr.MbrSignature)
            {
                break;
            }

            byte logicalSystemId = sector[LogicalEntryOffset + Mbr.EntrySystemIdOffset];
            uint logicalRelativeStart = BitConverter.ToUInt32(sector.Slice(LogicalEntryOffset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes));
            uint logicalSectorCount = BitConverter.ToUInt32(sector.Slice(LogicalEntryOffset + Mbr.EntrySectorCountOffset, Mbr.LbaFieldSizeBytes));

            byte nextSystemId = sector[NextEbrEntryOffset + Mbr.EntrySystemIdOffset];
            uint nextRelative = (nextSystemId == Mbr.SystemIdExtendedChs
                    || nextSystemId == Mbr.SystemIdExtendedLba
                    || nextSystemId == Mbr.SystemIdLinuxExtended)
                ? BitConverter.ToUInt32(sector.Slice(NextEbrEntryOffset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes))
                : 0u;

            // A relative start of 0 aliases the EBR sector itself; a range
            // past the envelope authorizes wild host I/O. Skip the entry
            // but keep walking — later links may still be intact.
            bool entryValid = logicalSystemId != Mbr.SystemIdEmpty
                && logicalRelativeStart != 0
                && logicalSectorCount != 0
                && currentEbrLba + logicalRelativeStart + logicalSectorCount <= envelopeEnd;
            if (entryValid)
            {
                nodes.Add(new ChainNode
                {
                    EbrLba = currentEbrLba,
                    LogicalSystemId = logicalSystemId,
                    LogicalRelativeStart = logicalRelativeStart,
                    LogicalSectorCount = logicalSectorCount,
                    NextRelative = nextRelative,
                });
            }

            if (nextRelative == 0)
            {
                break;
            }

            currentEbrLba = extendedStartSector + nextRelative;
            hops++;
        }

        return nodes;
    }

    private static void WriteEbrSector(
        IBlockDevice device,
        ulong ebrLba,
        byte logicalSystemId,
        uint relativeStart,
        uint sectorCount,
        uint nextRelative)
    {
        Span<byte> sector = new byte[device.BlockSize];

        sector[LogicalEntryOffset + Mbr.EntrySystemIdOffset] = logicalSystemId;
        BitConverter.TryWriteBytes(sector.Slice(LogicalEntryOffset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes), relativeStart);
        BitConverter.TryWriteBytes(sector.Slice(LogicalEntryOffset + Mbr.EntrySectorCountOffset, Mbr.LbaFieldSizeBytes), sectorCount);

        if (nextRelative != 0)
        {
            sector[NextEbrEntryOffset + Mbr.EntrySystemIdOffset] = Mbr.SystemIdExtendedChs;
            BitConverter.TryWriteBytes(sector.Slice(NextEbrEntryOffset + Mbr.EntryStartLbaOffset, Mbr.LbaFieldSizeBytes), nextRelative);
        }

        BitConverter.TryWriteBytes(sector.Slice(Mbr.SignatureOffset, Mbr.SignatureSizeBytes), Mbr.MbrSignature);

        device.WriteBlock(ebrLba, 1, sector);
    }

    private static ulong ResolveExtendedCount(IBlockDevice device, ulong extendedStartSector)
    {
        // The MBR extended entry is on-disk metadata: clamp its claimed
        // count to the device end. When the lookup cannot confirm the
        // envelope, grant nothing — a whole-disk fallback would let
        // mutators grow a logical into whatever follows the extended
        // partition.
        if (Mbr.TryGetExtendedPartition(device, out ulong start, out ulong count)
            && start == extendedStartSector
            && extendedStartSector < device.BlockCount)
        {
            ulong maxCount = device.BlockCount - extendedStartSector;
            return count > maxCount ? maxCount : count;
        }
        return 0;
    }
}
