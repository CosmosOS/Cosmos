// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.X64.Bridge;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// Adds MMIO mappings into Limine's existing page tables on demand, the x64
/// counterpart of the ARM64 <c>DeviceMapper</c>. Limine's blanket map (base
/// revision 0) covers the low 4 GiB plus memory-map regions, so MMIO below
/// 4 GiB is already reachable through the HHDM — but 64-bit BARs relocated
/// above 4 GiB are in no memory-map region and their HHDM alias page-faults.
/// This class walks CR3's 4-level tables and inserts 2 MiB uncacheable
/// (PCD|PWT) mappings so <c>phys + HHDM offset</c> dereferences work for
/// any BAR placement. <see cref="EnsureMapped"/> leaves already-present
/// mappings untouched: the low 4 GiB keeps Limine's attributes (MTRRs make
/// QEMU/PC MMIO uncacheable there, and rewriting live Limine entries under
/// registers is not worth the risk). A block Limine mapped in 4 KiB pages
/// may be mapped only in part: Limine maps the boot framebuffer to its last
/// byte, so the block holding its end has a table with the framebuffer's
/// pages and nothing after them. The pages such a table leaves out are
/// mapped uncacheable too, and the ones it maps keep Limine's attributes.
/// <para>
/// A framebuffer block is mapped write-combining instead
/// (<see cref="EnsureWriteCombining"/>), through the PAT entry that holds
/// WC, so stores to it leave in bursts rather than one bus transaction
/// each. Such a block belongs to one framebuffer whole, so the mappings
/// already there are changed too, Limine's included: a 1 GiB page of the
/// low 4 GiB is first split into 2 MiB pages with its own attributes.
/// </para>
/// </summary>
public static unsafe class DeviceMapper
{
    private const ulong FlagPresent = 1UL << 0;
    private const ulong FlagWritable = 1UL << 1;
    private const ulong FlagWriteThrough = 1UL << 3;
    private const ulong FlagCacheDisable = 1UL << 4;
    private const ulong FlagPageSize = 1UL << 7;
    private const ulong FlagNoExecute = 1UL << 63;

    /// <summary>The PAT index's high bit in a 4 KiB page entry.</summary>
    private const ulong FlagSmallPat = 1UL << 7;
    /// <summary>The PAT index's high bit in a 2 MiB or 1 GiB page entry, where bit 7 is the page size.</summary>
    private const ulong FlagLargePat = 1UL << 12;
    /// <summary>The bits of a 4 KiB page entry that pick its PAT entry.</summary>
    private const ulong SmallCacheMask = FlagWriteThrough | FlagCacheDisable | FlagSmallPat;
    /// <summary>The bits of a 2 MiB page entry that pick its PAT entry.</summary>
    private const ulong LargeCacheMask = FlagWriteThrough | FlagCacheDisable | FlagLargePat;

    /// <summary>The <c>IA32_PAT</c> MSR: eight memory types, one per byte, picked by an entry's PAT, PCD and PWT bits.</summary>
    private const uint PatMsr = 0x277;
    /// <summary>The PAT's encoding of the write-combining memory type.</summary>
    private const ulong PatWriteCombining = 0x01;
    /// <summary>Mask of one PAT entry's memory type.</summary>
    private const ulong PatTypeMask = 0x07;
    /// <summary>Entries in the PAT.</summary>
    private const int PatEntries = 8;
    /// <summary>Bits per PAT entry.</summary>
    private const int PatEntryBits = 8;
    /// <summary>The bit of a PAT index an entry's PWT sets.</summary>
    private const int PatIndexWriteThrough = 1;
    /// <summary>The bit of a PAT index an entry's PCD sets.</summary>
    private const int PatIndexCacheDisable = 2;
    /// <summary>The bit of a PAT index an entry's PAT bit sets.</summary>
    private const int PatIndexPat = 4;
    /// <summary><see cref="s_writeCombiningIndex"/> before the PAT was read.</summary>
    private const int PatUnread = -2;
    /// <summary><see cref="s_writeCombiningIndex"/> when no PAT entry holds write-combining.</summary>
    private const int PatWithoutWriteCombining = -1;

    // Physical-address field of a table entry (bits 51:12).
    private const ulong AddrMask = 0x000F_FFFF_FFFF_F000;
    // Physical-address field of a 1 GiB page entry (bits 51:30).
    private const ulong HugeAddrMask = 0x000F_FFFF_C000_0000;
    // 2 MiB alignment of a physical address (low 21 bits cleared).
    private const ulong Align2MiB = 0xFFFF_FFFF_FFE0_0000;
    /// <summary>Bytes a 2 MiB page entry maps.</summary>
    private const ulong LargePageSize = 0x20_0000;

    /// <summary>Right shift extracting the PML4 index from a virtual address (bits 47:39).</summary>
    private const int Pml4Shift = 39;
    /// <summary>Right shift extracting the PDPT index from a virtual address (bits 38:30).</summary>
    private const int PdptShift = 30;
    /// <summary>Right shift extracting the PD index from a virtual address (bits 29:21).</summary>
    private const int PdShift = 21;
    /// <summary>Mask isolating a 9-bit page-table index (512 entries per table).</summary>
    private const ulong TableIndexMask = 0x1FF;
    /// <summary>Entries in one page table.</summary>
    private const int TableEntries = 512;
    /// <summary>Bytes in one page a page table maps.</summary>
    private const ulong SmallPageSize = 0x1000;

    /// <summary>
    /// Serializes page-table walks and edits. Without it, two threads
    /// mapping blocks under the same empty PML4 or PDPT slot can each
    /// allocate a table and link it: the second link overwrites the first,
    /// and the mappings made through the first table vanish with it.
    /// IRQ-safe so a preemption never parks a holder mid-edit while another
    /// thread spins on it. Lock order: this lock may take the
    /// <see cref="PageAllocator"/> lock beneath it (table allocation), never
    /// the reverse, and nothing under it may call <see cref="EnsureMapped"/>
    /// or <see cref="EnsureWriteCombining"/> again: the lock is not reentrant.
    /// </summary>
    private static SchedSpinLock s_lock;

    /// <summary>The PAT entry holding write-combining, <see cref="PatWithoutWriteCombining"/> when none does, or <see cref="PatUnread"/>. Read under <see cref="s_lock"/>.</summary>
    private static int s_writeCombiningIndex = PatUnread;

    /// <summary>
    /// Ensures the 2 MiB block containing <paramref name="physBase"/> is
    /// mapped at (phys + HHDM offset). No-op when the block is already
    /// mapped (in particular the whole Limine-covered low 4 GiB). Safe to
    /// call multiple times and from concurrent threads.
    /// </summary>
    /// <returns>
    /// True when the block is mapped on return, whether this call installed
    /// the mapping or found one; false when there is no HHDM or a page-table
    /// allocation failed.
    /// </returns>
    public static bool EnsureMapped(ulong physBase)
    {
        return EnsureBlockMapped(physBase, writeCombining: false);
    }

    /// <summary>
    /// Ensures the 2 MiB block containing <paramref name="physBase"/> is
    /// mapped at (phys + HHDM offset) write-combining, for a framebuffer:
    /// a new mapping is installed so, and the mappings already there are
    /// changed to it, a 1 GiB page being split first. The whole block must
    /// belong to the framebuffer, since registers in it would lose their
    /// ordering. Mapped uncacheable, as <see cref="EnsureMapped"/>
    /// maps it, when no PAT entry holds write-combining (logged once). Safe
    /// to call multiple times and from concurrent threads.
    /// </summary>
    /// <returns>
    /// True when the block is mapped on return; false when there is no HHDM
    /// or a page-table allocation failed.
    /// </returns>
    public static bool EnsureWriteCombining(ulong physBase)
    {
        return EnsureBlockMapped(physBase, writeCombining: true);
    }

    private static bool EnsureBlockMapped(ulong physBase, bool writeCombining)
    {
        if (Limine.HHDM.Response == null)
        {
            return false;
        }

        ulong hhdm = Limine.HHDM.Response->Offset;
        ulong alignedPhys = physBase & Align2MiB;
        ulong virt = alignedPhys + hhdm;

        using (s_lock.AcquireIrqSafe())
        {
            int patIndex = writeCombining ? WriteCombiningIndex() : PatWithoutWriteCombining;
            return MapBlock(alignedPhys, virt, hhdm, patIndex);
        }
    }

    /// <summary>
    /// Walks CR3's tables to the PD slot covering <paramref name="virt"/>
    /// and installs a 2 MiB mapping there when nothing maps it yet, or fills
    /// in the pages a 4 KiB table there leaves out: uncacheable, or through
    /// PAT entry <paramref name="patIndex"/> when it names one (0 to 7), in
    /// which case the mappings already there take it too. Caller holds
    /// <see cref="s_lock"/>.
    /// </summary>
    private static bool MapBlock(ulong alignedPhys, ulong virt, ulong hhdm, int patIndex)
    {
        // The tables themselves live in low RAM, which the HHDM covers.
        ulong* pml4 = (ulong*)((X64CpuNative.ReadCr3() & AddrMask) + hhdm);

        // A PML4 entry cannot be a huge page (PS is reserved there), so
        // null here can only mean the table allocation failed.
        ulong* pdpt = GetOrCreateTable(pml4, (int)((virt >> Pml4Shift) & TableIndexMask), hhdm);
        if (pdpt == null)
        {
            return false;
        }

        bool recache = patIndex >= 0;
        int pdptIndex = (int)((virt >> PdptShift) & TableIndexMask);
        ulong pdptEntry = pdpt[pdptIndex];
        if ((pdptEntry & FlagPresent) != 0 && (pdptEntry & FlagPageSize) != 0)
        {
            // 1 GiB page already covers this block (Limine's low-4-GiB map).
            if (!recache)
            {
                return true;
            }

            if (!SplitHugePage(pdpt, pdptIndex, virt))
            {
                return false;
            }
        }

        // The 1 GiB case returned or was split above, so null is an
        // allocation failure.
        ulong* pd = GetOrCreateTable(pdpt, pdptIndex, hhdm);
        if (pd == null)
        {
            return false;
        }

        int pdIndex = (int)((virt >> PdShift) & TableIndexMask);
        ulong pdEntry = pd[pdIndex];
        if ((pdEntry & FlagPresent) != 0)
        {
            // A 2 MiB page maps the whole block; a 4 KiB table may map only
            // part of it.
            if ((pdEntry & FlagPageSize) == 0)
            {
                FillTable((ulong*)((pdEntry & AddrMask) + hhdm), alignedPhys, virt, patIndex);
            }
            else if (recache)
            {
                pd[pdIndex] = (pdEntry & ~LargeCacheMask) | LargeCacheBits(patIndex);
                X64CpuNative.InvalidatePage(virt);
            }

            return true;
        }

        Serial.WriteString("[DeviceMapper] Mapping MMIO phys 0x");
        Serial.WriteHex(alignedPhys);
        Serial.WriteString(" -> virt 0x");
        Serial.WriteHex(virt);
        Serial.WriteString(recache ? " (2MiB, WC)\n" : " (2MiB, UC)\n");

        // Uncacheable (PCD|PWT -> PAT UC) and non-executable: device
        // registers must not be prefetched, combined, or fetched as code.
        // A framebuffer's block is write-combining instead.
        pd[pdIndex] = alignedPhys | FlagPresent | FlagWritable
                    | (recache ? LargeCacheBits(patIndex) : FlagCacheDisable | FlagWriteThrough)
                    | FlagPageSize | FlagNoExecute;
        X64CpuNative.InvalidatePage(virt);
        return true;
    }

    /// <summary>
    /// Maps the pages of the 4 KiB table <paramref name="table"/> that map
    /// nothing yet, uncacheable and non-executable like a 2 MiB block, and
    /// leaves the present ones alone; with a PAT entry in
    /// <paramref name="patIndex"/>, every page is mapped through it, the
    /// present ones too. The table maps the 2 MiB block at
    /// <paramref name="alignedPhys"/>, seen at <paramref name="virt"/>.
    /// Silent when the table was full. Caller holds <see cref="s_lock"/>.
    /// </summary>
    private static void FillTable(ulong* table, ulong alignedPhys, ulong virt, int patIndex)
    {
        bool recache = patIndex >= 0;
        ulong cacheBits = recache ? SmallCacheBits(patIndex) : FlagCacheDisable | FlagWriteThrough;
        int filled = 0;
        for (int i = 0; i < TableEntries; i++)
        {
            ulong offset = (ulong)i * SmallPageSize;
            if ((table[i] & FlagPresent) != 0)
            {
                if (recache)
                {
                    table[i] = (table[i] & ~SmallCacheMask) | cacheBits;
                    X64CpuNative.InvalidatePage(virt + offset);
                }

                continue;
            }

            table[i] = (alignedPhys + offset) | FlagPresent | FlagWritable
                     | cacheBits | FlagNoExecute;
            X64CpuNative.InvalidatePage(virt + offset);
            filled++;
        }

        if (filled == 0)
        {
            return;
        }

        Serial.WriteString("[DeviceMapper] Mapping MMIO phys 0x");
        Serial.WriteHex(alignedPhys);
        Serial.WriteString(" -> virt 0x");
        Serial.WriteHex(virt);
        Serial.WriteString(" (");
        Serial.WriteNumber(filled);
        Serial.WriteString(" of ");
        Serial.WriteNumber(TableEntries);
        Serial.WriteString(recache ? " 4KiB pages, WC)\n" : " 4KiB pages, UC)\n");
    }

    /// <summary>
    /// Replaces the 1 GiB page at <paramref name="pdpt"/>[<paramref name="index"/>]
    /// with a table of 2 MiB pages that map the same memory with the same
    /// attributes (a 2 MiB entry keeps its PAT bit where a 1 GiB entry
    /// does), so one block of it can then take other attributes. The old
    /// translation is invalidated before any block changes, as a page size
    /// change asks. Caller holds <see cref="s_lock"/>.
    /// </summary>
    /// <returns>False when the table could not be allocated; the 1 GiB page stays.</returns>
    private static bool SplitHugePage(ulong* pdpt, int index, ulong virt)
    {
        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Serial.WriteString("[DeviceMapper] ERROR: page-table allocation failed\n");
            return false;
        }

        ulong entry = pdpt[index];
        ulong phys = entry & HugeAddrMask;
        ulong flags = entry & ~HugeAddrMask;
        ulong* pd = (ulong*)page;
        for (int i = 0; i < TableEntries; i++)
        {
            pd[i] = (phys + (ulong)i * LargePageSize) | flags;
        }

        pdpt[index] = PageAllocator.VirtualToPhysical((ulong)page) | FlagPresent | FlagWritable;
        X64CpuNative.InvalidatePage(virt);

        Serial.WriteString("[DeviceMapper] Split the 1GiB page at phys 0x");
        Serial.WriteHex(phys);
        Serial.WriteString(" into 2MiB pages\n");
        return true;
    }

    /// <summary>
    /// The PAT entry that holds write-combining, read from <c>IA32_PAT</c>
    /// once; Limine's protocol puts it at 5. <see cref="PatWithoutWriteCombining"/>
    /// when no entry holds it, logged once. Caller holds <see cref="s_lock"/>.
    /// </summary>
    private static int WriteCombiningIndex()
    {
        if (s_writeCombiningIndex != PatUnread)
        {
            return s_writeCombiningIndex;
        }

        ulong pat = X64CpuNative.ReadMsr(PatMsr);
        s_writeCombiningIndex = PatWithoutWriteCombining;
        for (int i = 0; i < PatEntries; i++)
        {
            if (((pat >> (i * PatEntryBits)) & PatTypeMask) == PatWriteCombining)
            {
                s_writeCombiningIndex = i;
                break;
            }
        }

        if (s_writeCombiningIndex == PatWithoutWriteCombining)
        {
            Serial.WriteString("[DeviceMapper] No PAT entry holds write-combining (IA32_PAT 0x");
            Serial.WriteHex(pat);
            Serial.WriteString("); framebuffers are mapped uncacheable\n");
        }

        return s_writeCombiningIndex;
    }

    /// <summary>The PWT, PCD and PAT bits of a 4 KiB page entry that pick PAT entry <paramref name="patIndex"/>.</summary>
    private static ulong SmallCacheBits(int patIndex)
    {
        return CacheBits(patIndex, FlagSmallPat);
    }

    /// <summary>The PWT, PCD and PAT bits of a 2 MiB page entry that pick PAT entry <paramref name="patIndex"/>.</summary>
    private static ulong LargeCacheBits(int patIndex)
    {
        return CacheBits(patIndex, FlagLargePat);
    }

    private static ulong CacheBits(int patIndex, ulong patFlag)
    {
        ulong bits = 0;
        if ((patIndex & PatIndexWriteThrough) != 0)
        {
            bits |= FlagWriteThrough;
        }

        if ((patIndex & PatIndexCacheDisable) != 0)
        {
            bits |= FlagCacheDisable;
        }

        if ((patIndex & PatIndexPat) != 0)
        {
            bits |= patFlag;
        }

        return bits;
    }

    /// <summary>
    /// Follows <paramref name="parent"/>[<paramref name="index"/>] to its
    /// child table, allocating and linking a zeroed one when the entry is
    /// not present. Returns null when the entry is a huge page (caller
    /// handles that as already-mapped) or the allocation fails. Caller
    /// holds <see cref="s_lock"/>.
    /// </summary>
    private static ulong* GetOrCreateTable(ulong* parent, int index, ulong hhdm)
    {
        ulong entry = parent[index];
        if ((entry & FlagPresent) != 0)
        {
            if ((entry & FlagPageSize) != 0)
            {
                return null;
            }

            return (ulong*)((entry & AddrMask) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Serial.WriteString("[DeviceMapper] ERROR: page-table allocation failed\n");
            return null;
        }

        ulong tablePhys = PageAllocator.VirtualToPhysical((ulong)page);
        parent[index] = tablePhys | FlagPresent | FlagWritable;
        return (ulong*)page;
    }
}
