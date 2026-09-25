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
/// any BAR placement. Already-present mappings are left untouched: the low
/// 4 GiB keeps Limine's attributes (MTRRs make QEMU/PC MMIO correct there
/// today, and rewriting live Limine entries is not worth the risk).
/// </summary>
public static unsafe class DeviceMapper
{
    private const ulong FlagPresent = 1UL << 0;
    private const ulong FlagWritable = 1UL << 1;
    private const ulong FlagWriteThrough = 1UL << 3;
    private const ulong FlagCacheDisable = 1UL << 4;
    private const ulong FlagPageSize = 1UL << 7;
    private const ulong FlagNoExecute = 1UL << 63;

    // Physical-address field of a table entry (bits 51:12).
    private const ulong AddrMask = 0x000F_FFFF_FFFF_F000;
    // 2 MiB alignment of a physical address (low 21 bits cleared).
    private const ulong Align2MiB = 0xFFFF_FFFF_FFE0_0000;

    /// <summary>Right shift extracting the PML4 index from a virtual address (bits 47:39).</summary>
    private const int Pml4Shift = 39;
    /// <summary>Right shift extracting the PDPT index from a virtual address (bits 38:30).</summary>
    private const int PdptShift = 30;
    /// <summary>Right shift extracting the PD index from a virtual address (bits 29:21).</summary>
    private const int PdShift = 21;
    /// <summary>Mask isolating a 9-bit page-table index (512 entries per table).</summary>
    private const ulong TableIndexMask = 0x1FF;

    /// <summary>
    /// Serializes page-table walks and edits. Without it, two threads
    /// mapping blocks under the same empty PML4 or PDPT slot can each
    /// allocate a table and link it: the second link overwrites the first,
    /// and the mappings made through the first table vanish with it.
    /// IRQ-safe so a preemption never parks a holder mid-edit while another
    /// thread spins on it. Lock order: this lock may take the
    /// <see cref="PageAllocator"/> lock beneath it (table allocation), never
    /// the reverse, and nothing under it may call <see cref="EnsureMapped"/>
    /// again: the lock is not reentrant.
    /// </summary>
    private static SchedSpinLock s_lock;

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
        if (Limine.HHDM.Response == null)
        {
            return false;
        }

        ulong hhdm = Limine.HHDM.Response->Offset;
        ulong alignedPhys = physBase & Align2MiB;
        ulong virt = alignedPhys + hhdm;

        using (s_lock.AcquireIrqSafe())
        {
            return MapBlock(alignedPhys, virt, hhdm);
        }
    }

    /// <summary>
    /// Walks CR3's tables to the PD slot covering <paramref name="virt"/>
    /// and installs a 2 MiB UC mapping there when nothing maps it yet.
    /// Caller holds <see cref="s_lock"/>.
    /// </summary>
    private static bool MapBlock(ulong alignedPhys, ulong virt, ulong hhdm)
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

        int pdptIndex = (int)((virt >> PdptShift) & TableIndexMask);
        ulong pdptEntry = pdpt[pdptIndex];
        if ((pdptEntry & FlagPresent) != 0 && (pdptEntry & FlagPageSize) != 0)
        {
            // 1 GiB page already covers this block (Limine's low-4-GiB map).
            return true;
        }

        // The 1 GiB case returned above, so null is an allocation failure.
        ulong* pd = GetOrCreateTable(pdpt, pdptIndex, hhdm);
        if (pd == null)
        {
            return false;
        }

        int pdIndex = (int)((virt >> PdShift) & TableIndexMask);
        if ((pd[pdIndex] & FlagPresent) != 0)
        {
            // A 2 MiB page or a 4 KiB table already maps this block.
            return true;
        }

        Serial.WriteString("[DeviceMapper] Mapping MMIO phys 0x");
        Serial.WriteHex(alignedPhys);
        Serial.WriteString(" -> virt 0x");
        Serial.WriteHex(virt);
        Serial.WriteString(" (2MiB, UC)\n");

        // Uncacheable (PCD|PWT -> PAT UC) and non-executable: device
        // registers must not be prefetched, combined, or fetched as code.
        pd[pdIndex] = alignedPhys | FlagPresent | FlagWritable
                    | FlagCacheDisable | FlagWriteThrough
                    | FlagPageSize | FlagNoExecute;
        X64CpuNative.InvalidatePage(virt);
        return true;
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
