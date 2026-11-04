// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.RISCV64.Bridge;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

/// <summary>
/// Adds device MMIO mappings into Limine's page tables (the satp root).
/// On RISC-V a page table entry carries no memory type: the platform's
/// physical memory attributes make a device range uncached and strongly
/// ordered whatever maps it (short of the Svpbmt extension, which the kernel
/// does not use). So the HHDM alias of a device address only needs to exist:
/// Limine's revision 0 maps the first 4 GiB there, and this class adds a
/// 2 MiB megapage for a device above that (a 64-bit PCI window).
/// </summary>
public static unsafe class DeviceMapper
{
    /// <summary>PTE.V: the entry is valid.</summary>
    private const ulong PteValid = 1UL << 0;
    /// <summary>PTE.R: readable; R, W or X set makes the entry a leaf.</summary>
    private const ulong PteRead = 1UL << 1;
    /// <summary>PTE.W: writable.</summary>
    private const ulong PteWrite = 1UL << 2;
    /// <summary>PTE.X: executable.</summary>
    private const ulong PteExecute = 1UL << 3;
    /// <summary>PTE.G: global, in every address space.</summary>
    private const ulong PteGlobal = 1UL << 5;
    /// <summary>PTE.A: accessed. Set up front: hardware without A/D updates faults on a clear bit.</summary>
    private const ulong PteAccessed = 1UL << 6;
    /// <summary>PTE.D: dirty. Set up front, as A.</summary>
    private const ulong PteDirty = 1UL << 7;
    /// <summary>The bits that make an entry a leaf.</summary>
    private const ulong PteLeafMask = PteRead | PteWrite | PteExecute;
    /// <summary>The PPN field: bits 53:10 of an entry, the physical page number.</summary>
    private const ulong PtePpnMask = 0x003F_FFFF_FFFF_FC00UL;
    /// <summary>Where the PPN starts in an entry.</summary>
    private const int PtePpnShift = 10;

    /// <summary>satp.MODE: bits 63:60.</summary>
    private const int SatpModeShift = 60;
    /// <summary>satp.PPN: bits 43:0, the root table's physical page number.</summary>
    private const ulong SatpPpnMask = 0x0000_0FFF_FFFF_FFFFUL;
    /// <summary>satp.MODE of Sv39: three levels.</summary>
    private const ulong SatpModeSv39 = 8;
    /// <summary>satp.MODE of Sv48: four levels.</summary>
    private const ulong SatpModeSv48 = 9;
    /// <summary>satp.MODE of Sv57: five levels.</summary>
    private const ulong SatpModeSv57 = 10;

    /// <summary>Bytes per page.</summary>
    private const int PageShift = 12;
    /// <summary>Bytes a level-1 leaf (a megapage) maps.</summary>
    private const ulong MegapageSize = 1UL << 21;
    /// <summary>Entries in a page table: 4 KiB of 8-byte entries.</summary>
    private const int TableEntries = 512;

    /// <summary>
    /// Serializes page table walks and edits, as on the other architectures.
    /// IRQ-safe; may take the PageAllocator lock beneath it, never the
    /// reverse, and is not reentrant.
    /// </summary>
    private static SchedSpinLock s_lock;

    /// <summary>
    /// Ensures a physical MMIO address is mapped at its HHDM alias
    /// (phys + HHDM offset). Safe to call more than once and from concurrent
    /// threads; a no-op when a mapping already covers the address.
    /// </summary>
    /// <returns>
    /// True when the address is mapped on return; false when there is no
    /// HHDM, translation is off or a page table could not be allocated.
    /// </returns>
    public static bool EnsureMapped(ulong physBase)
    {
        if (Limine.HHDM.Response == null)
        {
            return false;
        }

        ulong hhdm = Limine.HHDM.Response->Offset;
        using (s_lock.AcquireIrqSafe())
        {
            return MapMegapage(physBase, hhdm);
        }
    }

    /// <summary>
    /// Walks the tables to the level-1 entry covering the alias of
    /// <paramref name="physBase"/>, creating the tables that are missing,
    /// and installs a 2 MiB read/write megapage there unless a leaf already
    /// maps the address. Caller holds <see cref="s_lock"/>.
    /// </summary>
    private static bool MapMegapage(ulong physBase, ulong hhdmOffset)
    {
        ulong satp = CsrNative.ReadSatp();
        int levels = (satp >> SatpModeShift) switch
        {
            SatpModeSv39 => 3,
            SatpModeSv48 => 4,
            SatpModeSv57 => 5,
            _ => 0,
        };

        if (levels == 0)
        {
            Serial.Write("[DeviceMapper] ERROR: translation is off (satp.MODE = Bare)\n");
            return false;
        }

        ulong aligned = physBase & ~(MegapageSize - 1);
        ulong virt = aligned + hhdmOffset;
        ulong tablePhys = (satp & SatpPpnMask) << PageShift;

        for (int level = levels - 1; level >= 1; level--)
        {
            ulong* table = (ulong*)(tablePhys + hhdmOffset);
            int index = (int)((virt >> (PageShift + (9 * level))) & (TableEntries - 1));
            ulong entry = table[index];

            if ((entry & PteValid) != 0 && (entry & PteLeafMask) != 0)
            {
                // A leaf at this level or above already maps the address:
                // the PMAs give it device semantics whatever its flags.
                return true;
            }

            if (level == 1)
            {
                table[index] = ((aligned >> PageShift) << PtePpnShift)
                               | PteValid | PteRead | PteWrite | PteGlobal | PteAccessed | PteDirty;
                CsrNative.SfenceVma(virt);

                Serial.Write("[DeviceMapper] Mapping phys 0x");
                Serial.WriteHex(aligned);
                Serial.Write(" -> virt 0x");
                Serial.WriteHex(virt);
                Serial.Write("\n");
                return true;
            }

            if ((entry & PteValid) == 0)
            {
                void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
                if (page == null)
                {
                    Serial.Write("[DeviceMapper] ERROR: page table allocation failed\n");
                    return false;
                }

                ulong pagePhys = PageAllocator.VirtualToPhysical((ulong)page);
                // A non-leaf entry: V only. Global, since the HHDM is in every address space.
                table[index] = ((pagePhys >> PageShift) << PtePpnShift) | PteValid | PteGlobal;
                entry = table[index];
            }

            tablePhys = ((entry & PtePpnMask) >> PtePpnShift) << PageShift;
        }

        return false;
    }
}
