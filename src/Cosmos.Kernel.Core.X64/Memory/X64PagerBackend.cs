// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.X64.Bridge;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 implementation of <see cref="IPagerBackend"/>: a four-level walk over the
/// PML4 → PDPT → PD → PT hierarchy that Limine left installed.
/// </summary>
/// <remarks>
/// This is the paging machinery <c>DeviceMapper</c> does a single fixed-purpose
/// version of. Where that one only ever inserts 2 MiB uncacheable entries at the
/// HHDM alias, this one installs arbitrary regions at 4 KiB or 2 MiB granularity
/// and translates them back, which is what <see cref="Pager"/> needs.
/// <para>
/// Two x64-specific details are worth knowing before changing anything here.
/// First, a huge page at an intermediate level (PS in a PDPT or PD entry) hides
/// everything below it, so a caller asking for 4 KiB under an existing 1 GiB
/// Limine mapping cannot be satisfied without splitting that leaf — this
/// backend reports failure rather than silently mapping the wrong granularity,
/// because splitting Limine's live leaves is a riskier change than it looks.
/// Second, PAT encodes the memory type, and this backend drives it through
/// PCD/PWT rather than programming the PAT MSR: <c>Device</c> sets both bits
/// (PAT index 6, uncacheable) and everything else leaves them clear (write-back),
/// which matches the assumption <c>DeviceMapper</c> already relies on.
/// </para>
/// </remarks>
internal sealed unsafe class X64PagerBackend : IPagerBackend
{
    // x64 page-entry bits (Intel SDM Vol 3A, Table 4-18).
    private const ulong FlagPresent = 1UL << 0;
    private const ulong FlagWritable = 1UL << 1;
    private const ulong FlagUser = 1UL << 2;
    private const ulong FlagWriteThrough = 1UL << 3;
    private const ulong FlagCacheDisable = 1UL << 4;
    private const ulong FlagAccessed = 1UL << 5;
    private const ulong FlagDirty = 1UL << 6;
    private const ulong FlagPageSize = 1UL << 7;
    private const ulong FlagGlobal = 1UL << 8;
    private const ulong FlagNoExecute = 1UL << 63;

    /// <summary>Physical-address field of an entry (bits 51:12).</summary>
    private const ulong AddrMask = 0x000F_FFFF_FFFF_F000;

    /// <summary>Mask selecting the 9-bit table index (512 entries per table).</summary>
    private const ulong TableIndexMask = 0x1FF;

    private const int Pml4Shift = 39;
    private const int PdptShift = 30;
    private const int PdShift = 21;
    private const int PtShift = 12;

    /// <summary>Physical frames a huge page covers at each level.</summary>
    private const ulong PagesPer2MiB = 512;

    /// <summary>
    /// Serializes page-table walks and edits. Without it, two threads mapping
    /// regions under the same empty PML4 or PDPT slot can each allocate a table
    /// and link it: the second link overwrites the first, and the mappings made
    /// through the first table vanish with it.
    /// <para>
    /// This is the same hazard and the same lock <c>DeviceMapper</c> guards, and
    /// deliberately the same instance concern in reverse: both writers edit the
    /// kernel's live tables, so a region one of them installs can be lost to a
    /// concurrent commit by the other. They do not share a lock, so overlapping
    /// the two is still unsafe — they only must not race on the same empty
    /// intermediate slot from two threads each.
    /// </para>
    /// <para>
    /// IRQ-safe so a preemption never parks a holder mid-edit while another
    /// thread spins on it. Lock order: this lock may take the
    /// <see cref="PageAllocator"/> lock beneath it (table allocation), never the
    /// reverse, and nothing under it may call <see cref="Commit"/> again: the
    /// lock is not reentrant.
    /// </para>
    /// </summary>
    private static SchedSpinLock s_lock;

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public ulong CurrentRootFrame() => X64CpuNative.ReadCr3() & AddrMask;

    /// <inheritdoc />
    /// <remarks>
    /// x86-64 has a single 4-level CR3 spanning the whole 48-bit address space,
    /// so both halves are served by the same root. Reporting 0 here tells
    /// <see cref="Pager.Create"/> to reuse <see cref="CurrentRootFrame"/>.
    /// </remarks>
    public ulong CurrentLowRootFrame() => 0;

    /// <inheritdoc />
    public void Commit(AddressSpaceDescriptor* space)
    {
        ulong rootFrame = space->RootFrame;
        if (rootFrame == 0)
        {
            return;
        }

        ulong hhdm = GetHhdmOffset();
        if (hhdm == 0)
        {
            // Without the HHDM there is no way to reach a page table through a
            // frame address, so there is nothing to walk. Limine always
            // answers this on x64, so reaching it means a real problem.
            Serial.WriteString("[Pager] Commit: no HHDM, cannot reach page tables\n");
            return;
        }

        ulong* pml4 = (ulong*)(rootFrame + hhdm);

        using (s_lock.AcquireIrqSafe())
        {
            for (int i = 0; i < space->RegionCount; i++)
            {
                ref AddressSpaceRegion region = ref space->Regions[i];
                InstallRegion(pml4, in region, hhdm);
            }
        }
    }

    /// <inheritdoc />
    public bool Translate(AddressSpaceDescriptor* space, VirtualAddress virt, out PhysicalAddress phys)
    {
        phys = PhysicalAddress.None;

        if (space->RootFrame == 0)
        {
            return false;
        }

        ulong hhdm = GetHhdmOffset();
        if (hhdm == 0)
        {
            return false;
        }

        ulong address = virt.Value;
        if ((address & (PageAllocator.PageSize - 1)) != 0)
        {
            // Unaligned: the answer would be ambiguous, and the caller almost
            // certainly has an arithmetic bug rather than a mapping question.
            return false;
        }

        ulong* pml4 = (ulong*)(space->RootFrame + hhdm);

        if (!Walk(pml4, address, hhdm, out ulong frame))
        {
            return false;
        }

        // A resolved frame must be page-aligned. If it is not, the walk
        // mis-decoded an entry; report failure rather than returning a bogus
        // frame, and never throw — callers include fault handlers.
        if ((frame & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <inheritdoc />
    public bool TranslateCurrent(VirtualAddress virt, out PhysicalAddress phys)
    {
        ulong rootFrame = CurrentRootFrame();
        ulong hhdm = GetHhdmOffset();

        if (rootFrame == 0 || hhdm == 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        ulong address = virt.Value;
        if ((address & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        if (!Walk((ulong*)(rootFrame + hhdm), address, hhdm, out ulong frame))
        {
            phys = PhysicalAddress.None;
            return false;
        }

        // A resolved frame must be page-aligned. If it is not, the walk
        // mis-decoded an entry; report failure rather than returning a bogus
        // frame, and never throw — callers include fault handlers.
        if ((frame & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <summary>
    /// Resolves a virtual address to a physical frame by walking all four
    /// levels, stopping at the first huge page it meets.
    /// </summary>
    /// <remarks>
    /// The HHDM offset is a parameter rather than re-read per level: a table
    /// frame that is non-zero must never be added to an offset that turned out
    /// to be 0, which would silently yield a bogus pointer.
    /// </remarks>
    private static bool Walk(ulong* pml4, ulong address, ulong hhdm, out ulong frame)
    {
        frame = 0;

        ulong pml4Entry = ReadEntry(pml4, (int)((address >> Pml4Shift) & TableIndexMask));
        if (pml4Entry == 0 || (pml4Entry & FlagPageSize) != 0)
        {
            return false;
        }

        ulong* pdpt = (ulong*)((pml4Entry & AddrMask) + hhdm);

        ulong pdptEntry = ReadEntry(pdpt, (int)((address >> PdptShift) & TableIndexMask));
        if (pdptEntry == 0)
        {
            return false;
        }

        if ((pdptEntry & FlagPageSize) != 0)
        {
            // 1 GiB page: the physical field holds the base frame, so the low
            // 30 bits of the virtual address select within it.
            frame = (pdptEntry & AddrMask) + (address & 0x3FFF_FFFF);
            return true;
        }

        ulong* pd = (ulong*)((pdptEntry & AddrMask) + hhdm);

        ulong pdEntry = ReadEntry(pd, (int)((address >> PdShift) & TableIndexMask));
        if (pdEntry == 0)
        {
            return false;
        }

        if ((pdEntry & FlagPageSize) != 0)
        {
            // 2 MiB page: bits 51:21 hold the frame, so mask off the 21 bits
            // that are virtual page offset rather than physical address.
            frame = (pdEntry & AddrMask) + (address & 0x1F_FFFF);
            return true;
        }

        ulong* pt = (ulong*)((pdEntry & AddrMask) + hhdm);

        ulong ptEntry = ReadEntry(pt, (int)((address >> PtShift) & TableIndexMask));
        if (ptEntry == 0)
        {
            return false;
        }

        frame = ptEntry & AddrMask;
        return true;
    }

    /// <summary>
    /// Reads one entry and returns it only when it is present, so callers can
    /// treat 0 as "not mapped" without re-checking the present bit.
    /// </summary>
    private static ulong ReadEntry(ulong* table, int index)
    {
        ulong entry = table[index];
        return (entry & FlagPresent) != 0 ? entry : 0;
    }

    /// <summary>
    /// Installs one region, choosing the largest granularity that fully covers
    /// it and allocating intermediate tables as needed.
    /// </summary>
    private static void InstallRegion(ulong* pml4, in AddressSpaceRegion region, ulong hhdm)
    {
        if (region.PageCount == 0)
        {
            return;
        }

        ulong virt = region.Virt.Value;
        ulong phys = region.Phys.Value;
        ulong attributes = EncodeFlags(region.Flags);

        // A 2 MiB region is described by a single PD entry, provided it is
        // aligned in both virtual and physical space. Anything else, including
        // an unaligned 2 MiB region, is split into 4 KiB pages.
        if (region.PageCount == PagesPer2MiB && (virt & 0x1F_FFFF) == 0 && (phys & 0x1F_FFFF) == 0)
        {
            if (Install2MiB(pml4, virt, phys, attributes, hhdm))
            {
                X64CpuNative.InvalidatePage(virt);
                return;
            }
        }

        ulong cursor = virt;
        ulong remaining = region.PageCount;
        ulong frame = phys;

        while (remaining > 0)
        {
            if (!Install4KiB(pml4, cursor, frame, attributes, hhdm))
            {
                return;
            }

            X64CpuNative.InvalidatePage(cursor);
            cursor += PageAllocator.PageSize;
            frame += PageAllocator.PageSize;
            remaining--;
        }
    }

    /// <summary>
    /// Installs a single 4 KiB page, allocating any missing intermediate table.
    /// Returns false if an intermediate level is already covered by a huge page
    /// that would have to be split, or a table allocation fails.
    /// </summary>
    private static bool Install4KiB(ulong* pml4, ulong virt, ulong phys, ulong attributes, ulong hhdm)
    {
        ulong* pdpt = GetOrCreateTable(pml4, (int)((virt >> Pml4Shift) & TableIndexMask), hhdm);
        if (pdpt is null)
        {
            return false;
        }

        int pdptIndex = (int)((virt >> PdptShift) & TableIndexMask);
        ulong pdptEntry = ReadEntry(pdpt, pdptIndex);
        if (pdptEntry != 0 && (pdptEntry & FlagPageSize) != 0)
        {
            // A 1 GiB page covers this address. Splitting it would mean
            // rewriting a live Limine leaf, which is out of scope here.
            Serial.WriteString("[Pager] 4KiB map under a 1GiB leaf not supported: virt 0x");
            Serial.WriteHex(virt);
            Serial.WriteString("\n");
            return false;
        }

        ulong* pd = GetOrCreateTable(pdpt, pdptIndex, hhdm);
        if (pd is null)
        {
            return false;
        }

        int pdIndex = (int)((virt >> PdShift) & TableIndexMask);
        ulong pdEntry = ReadEntry(pd, pdIndex);
        if (pdEntry != 0 && (pdEntry & FlagPageSize) != 0)
        {
            // Same reasoning as the 1 GiB case, one level down.
            Serial.WriteString("[Pager] 4KiB map under a 2MiB leaf not supported: virt 0x");
            Serial.WriteHex(virt);
            Serial.WriteString("\n");
            return false;
        }

        ulong* pt = GetOrCreateTable(pd, pdIndex, hhdm);
        if (pt is null)
        {
            return false;
        }

        int ptIndex = (int)((virt >> PtShift) & TableIndexMask);
        pt[ptIndex] = phys | attributes | FlagPresent | FlagDirty | FlagAccessed;
        return true;
    }

    /// <summary>
    /// Installs a single 2 MiB page as one PD entry, or returns false when a
    /// deeper level already owns the address.
    /// </summary>
    private static bool Install2MiB(ulong* pml4, ulong virt, ulong phys, ulong attributes, ulong hhdm)
    {
        ulong* pdpt = GetOrCreateTable(pml4, (int)((virt >> Pml4Shift) & TableIndexMask), hhdm);
        if (pdpt is null)
        {
            return false;
        }

        int pdptIndex = (int)((virt >> PdptShift) & TableIndexMask);
        ulong pdptEntry = ReadEntry(pdpt, pdptIndex);
        if (pdptEntry != 0 && (pdptEntry & FlagPageSize) != 0)
        {
            // Already a 1 GiB page covering this range; nothing to do.
            return true;
        }

        ulong* pd = GetOrCreateTable(pdpt, pdptIndex, hhdm);
        if (pd is null)
        {
            return false;
        }

        int pdIndex = (int)((virt >> PdShift) & TableIndexMask);

        // PS is only meaningful on a PD entry; drop it from the passed-through
        // attributes so the two encodings cannot drift apart.
        pd[pdIndex] = phys | (attributes & ~FlagPageSize) | FlagPresent | FlagPageSize
                    | FlagAccessed | FlagDirty;

        return true;
    }

    /// <summary>
    /// Follows <paramref name="parent"/>[<paramref name="index"/>"] to its child
    /// table, allocating and linking a zeroed one when absent. Returns null when
    /// the entry is a huge page (caller decides how to handle that) or an
    /// allocation fails.
    /// </summary>
    private static ulong* GetOrCreateTable(ulong* parent, int index, ulong hhdm)
    {
        ulong entry = parent[index];
        if ((entry & FlagPresent) != 0)
        {
            return (entry & FlagPageSize) != 0
                ? null
                : (ulong*)((entry & AddrMask) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page is null)
        {
            Serial.WriteString("[Pager] ERROR: page-table allocation failed\n");
            return null;
        }

        ulong tableFrame = PageAllocator.VirtualToPhysical((ulong)page);

        // Intermediate tables are marked writable only. They must not inherit
        // PageFlags from a region: a table entry's own permission bits govern
        // the level above it, not the pages it describes.
        parent[index] = tableFrame | FlagPresent | FlagWritable;
        return (ulong*)page;
    }

    /// <summary>
    /// Translates the platform-agnostic <see cref="PageFlags"/> into x64 entry
    /// bits.
    /// </summary>
    /// <remarks>
    /// <see cref="PageFlags.Executable"/> is inverted into NX, and
    /// <see cref="PageFlags.Device"/> sets PCD+PWT — the PAT entry that means
    /// uncacheable on the platform PAT layout <c>DeviceMapper</c> already
    /// assumes. <see cref="PageFlags.Cacheable"/> and
    /// <see cref="PageFlags.Uncached"/> are not separately expressible that way:
    /// PAT here distinguishes only uncacheable from write-back, so
    /// <see cref="PageFlags.Uncached"/> falls through to the write-back default
    /// rather than silently claiming to have been honoured. Programming the PAT
    /// MSR is what would be needed to express it properly.
    /// </remarks>
    private static ulong EncodeFlags(PageFlags flags)
    {
        ulong bits = 0;

        if ((flags & PageFlags.Writable) != 0)
        {
            bits |= FlagWritable;
        }

        if ((flags & PageFlags.User) != 0)
        {
            bits |= FlagUser;
        }

        if ((flags & PageFlags.Executable) == 0)
        {
            bits |= FlagNoExecute;
        }

        if ((flags & PageFlags.Global) != 0)
        {
            bits |= FlagGlobal;
        }

        if ((flags & PageFlags.Device) != 0)
        {
            bits |= FlagCacheDisable | FlagWriteThrough;
        }

        return bits;
    }

    /// <summary>
    /// The HHDM offset, or 0 when Limine did not answer. Used to turn a page
    /// table's frame address into a dereferenceable pointer.
    /// </summary>
    private static ulong GetHhdmOffset()
    {
        return Limine.HHDM.Response is null
            ? 0
            : Limine.HHDM.Response->Offset;
    }
}