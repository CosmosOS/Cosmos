// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// The global graphics translation table (GGTT): the page table that turns
/// the GPU addresses a plane or a cursor scans out from, and the CPU's
/// accesses through the aperture in BAR 2, into physical pages. It sits in
/// the upper half of BAR 0, one entry per 4 KiB page: 32 bits before
/// Broadwell, 64 from Broadwell on, the formats of i915's
/// <c>snb_pte_encode</c>, <c>hsw_pte_encode</c> and
/// <c>gen8_ggtt_pte_encode</c>. An entry this driver writes carries the
/// valid and caching bits of an entry the firmware wrote, the one of the
/// surface it left on screen, so the frames the display flips to are cached
/// the way the firmware's own surface is. Thread context.
/// </summary>
internal sealed class IntelGraphicsGtt
{
    /// <summary>Bytes one entry maps.</summary>
    internal const ulong PageBytes = 4096;

    /// <summary>The valid bit of an entry, in every format.</summary>
    private const ulong EntryValid = 0x1;

    /// <summary>The page address bits 31:12 of a 32-bit entry.</summary>
    private const uint NarrowEntryLowMask = 0xFFFF_F000;

    /// <summary>Where a 32-bit entry keeps the page address bits above 31 before Haswell: bits 11:4 hold address bits 39:32.</summary>
    private const uint Gen6EntryHighMask = 0xFF0;

    /// <summary>Where a 32-bit entry keeps the page address bits above 31 on Haswell: bits 10:4 hold address bits 38:32.</summary>
    private const uint HaswellEntryHighMask = 0x7F0;

    /// <summary>The shift from those entry bits to the address bits above 31.</summary>
    private const int NarrowEntryHighShift = 28;

    /// <summary>The valid and caching bits of a 32-bit entry before Haswell: GEN6_PTE_VALID and the cache field, bits 3:1.</summary>
    private const uint Gen6EntryFlagsMask = 0x00F;

    /// <summary>The valid and caching bits of a 32-bit entry on Haswell: the cache field, bits 3:1, and its fourth bit, bit 11.</summary>
    private const uint HaswellEntryFlagsMask = 0x80F;

    /// <summary>GEN12_GGTT_PTE_ADDR_MASK: the page address of a 64-bit entry.</summary>
    private const ulong WideEntryAddressMask = 0x0000_3FFF_FFFF_F000;

    /// <summary>The valid and PAT bits of a 64-bit entry, below the page address.</summary>
    private const ulong WideEntryFlagsMask = 0xFFF;

    private readonly RegisterWindow _registers;
    private readonly IntelGraphicsGeneration _generation;
    private readonly ulong _tableOffset;
    private readonly ulong _entryBytes;

    /// <summary>Bytes of GPU address space the table maps.</summary>
    internal ulong Size { get; }

    /// <summary>Where a 32-bit entry keeps the address bits above 31 on this generation.</summary>
    private uint HighMask => _generation.Platform == IntelGraphicsPlatform.Haswell ? HaswellEntryHighMask : Gen6EntryHighMask;

    /// <summary>The valid and caching bits of a 32-bit entry on this generation.</summary>
    private uint FlagsMask => _generation.Platform == IntelGraphicsPlatform.Haswell ? HaswellEntryFlagsMask : Gen6EntryFlagsMask;

    /// <summary>The table in the upper half of BAR 0.</summary>
    /// <param name="registers">BAR 0, all of it.</param>
    /// <param name="generation">The engine's generation, which picks the entry format.</param>
    /// <param name="graphicsControl">GGC, which gives the table's size.</param>
    internal IntelGraphicsGtt(RegisterWindow registers, IntelGraphicsGeneration generation, ushort graphicsControl)
    {
        _registers = registers;
        _generation = generation;
        _tableOffset = registers.Length / 2;
        _entryBytes = (ulong)(generation.HasWideGttEntries ? sizeof(ulong) : sizeof(uint));

        ulong tableBytes;
        if (generation.HasWideGttEntries)
        {
            uint code = ((uint)graphicsControl >> IntelGraphicsRegisters.GttSizeShiftGen8) & IntelGraphicsRegisters.GttSizeMask;
            tableBytes = code == 0 ? 0 : (1UL << (int)code) * 1024 * 1024;
        }
        else
        {
            uint code = ((uint)graphicsControl >> IntelGraphicsRegisters.GttSizeShiftGen6) & IntelGraphicsRegisters.GttSizeMask;
            tableBytes = (ulong)code * 1024 * 1024;
        }

        // The table cannot run past the upper half of BAR 0, whatever GGC says.
        tableBytes = Math.Min(tableBytes, registers.Length - _tableOffset);
        Size = tableBytes / _entryBytes * PageBytes;
    }

    /// <summary>The physical page a GPU address maps to and the entry's valid and caching bits. Reads only.</summary>
    /// <param name="gpuAddress">A GPU address.</param>
    /// <param name="physicalAddress">The physical address the GPU address reaches.</param>
    /// <param name="flags">The entry's valid and caching bits, for <see cref="Map"/>.</param>
    /// <returns>False when the entry is not valid or the address is past the table.</returns>
    internal bool TryTranslate(ulong gpuAddress, out ulong physicalAddress, out ulong flags)
    {
        physicalAddress = 0;
        flags = 0;
        if (gpuAddress >= Size)
        {
            return false;
        }

        ulong offset = EntryOffset(gpuAddress);
        ulong page;
        if (_generation.HasWideGttEntries)
        {
            ulong entry = _registers.Read64(offset);
            page = entry & WideEntryAddressMask;
            flags = entry & WideEntryFlagsMask;
        }
        else
        {
            uint entry = _registers.Read32(offset);
            page = (entry & NarrowEntryLowMask) | ((ulong)(entry & HighMask) << NarrowEntryHighShift);
            flags = entry & FlagsMask;
        }

        if ((flags & EntryValid) == 0)
        {
            return false;
        }

        physicalAddress = page | (gpuAddress & (PageBytes - 1));
        return true;
    }

    /// <summary>
    /// Maps <paramref name="length"/> bytes of GPU address space at
    /// <paramref name="gpuAddress"/> onto the physical pages at
    /// <paramref name="physicalAddress"/>, then makes the GT see the new
    /// entries: the flush register before Ice Lake, as i915's
    /// <c>gen6_ggtt_invalidate</c> writes it, and a read back of the last
    /// entry, so the uncached writes have landed before a plane is pointed
    /// at them.
    /// </summary>
    /// <param name="gpuAddress">The first GPU address, page aligned, inside <see cref="Size"/>.</param>
    /// <param name="physicalAddress">The first physical address, page aligned.</param>
    /// <param name="length">Bytes to map, at least one, rounded up to a page.</param>
    /// <param name="flags">The valid and caching bits, as <see cref="TryTranslate"/> read them from the firmware's entry.</param>
    internal void Map(ulong gpuAddress, ulong physicalAddress, ulong length, ulong flags)
    {
        ulong last = gpuAddress;
        for (ulong mapped = 0; mapped < length; mapped += PageBytes)
        {
            last = gpuAddress + mapped;
            ulong offset = EntryOffset(last);
            ulong page = physicalAddress + mapped;
            if (_generation.HasWideGttEntries)
            {
                _registers.Write64(offset, page | flags);
            }
            else
            {
                uint entry = ((uint)page & NarrowEntryLowMask) | ((uint)(page >> NarrowEntryHighShift) & HighMask) | (uint)flags;
                _registers.Write32(offset, entry);
            }
        }

        if (_generation.FlushesGttWrites)
        {
            _registers.Write32(IntelGraphicsRegisters.GraphicsFlushControl, IntelGraphicsRegisters.GraphicsFlushEnable);
            _registers.Read32(IntelGraphicsRegisters.GraphicsFlushControl);
        }

        TryTranslate(last, out _, out _);
    }

    private ulong EntryOffset(ulong gpuAddress)
    {
        return _tableOffset + gpuAddress / PageBytes * _entryBytes;
    }
}
