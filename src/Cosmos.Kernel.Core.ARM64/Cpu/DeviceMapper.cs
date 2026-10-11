// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// Adds Device MMIO mappings into Limine's existing TTBR1 page tables.
/// Limine's HHDM only covers RAM; device MMIO regions (GIC, timers, etc.)
/// are NOT mapped. This class walks the TTBR1 page tables and inserts
/// 2MiB block descriptors with Device-nGnRnE/nGnRE attributes so the
/// HHDM virtual address (phys + HHDM_OFFSET) can be dereferenced for MMIO.
/// Native imports live in Cosmos.Kernel.Core.ARM64/Bridge/Import/DeviceMapperNative.cs.
/// </summary>
public static unsafe class DeviceMapper
{
    // Page table descriptor bits
    private const ulong DESC_VALID = 1UL << 0;
    private const ulong DESC_TABLE = 1UL << 1;  // 1 = table, 0 = block
    private const ulong DESC_AF = 1UL << 10;
    private const ulong DESC_PXN = 1UL << 53;
    private const ulong DESC_UXN = 1UL << 54;
    private const ulong ADDR_MASK = 0x0000FFFFFFFFF000UL;
    private const ulong BLOCK_2MB_ADDR_MASK = 0x0000FFFFFFE00000UL;
    private const ulong BLOCK_1GB_ADDR_MASK = 0x0000FFFFC0000000UL;

    private static bool s_spareL2Used;

    /// <summary>
    /// Serializes page-table walks and edits, including the check-then-set
    /// of <see cref="s_spareL2Used"/>: two threads splitting at once would
    /// both fill the single spare L2 table for different 1 GiB blocks, and
    /// one L1 slot would end up pointing at the other's addresses. IRQ-safe
    /// so a preemption never parks a holder mid-edit while another thread
    /// spins on it. Lock order: this lock may take the PageAllocator lock
    /// beneath it, never the reverse, and nothing under it may call
    /// <see cref="EnsureMapped"/> again: the lock is not reentrant.
    /// </summary>
    private static SchedSpinLock s_lock;

    // ── Public API ──────────────────────────────────────────────────

    /// <summary>
    /// Ensures a physical MMIO address is mapped as Device memory in the
    /// TTBR1 page tables so it can be accessed at (phys + HHDM_OFFSET).
    /// Safe to call multiple times and from concurrent threads; no-ops if
    /// the mapping already exists.
    /// </summary>
    /// <returns>
    /// True when the 2 MiB block is Device-mapped on return, whether this
    /// call installed the mapping or found one; false when there is no HHDM
    /// or the tables cannot reach the block (missing L0/L1 entry, no spare
    /// L2 table left to split a 1 GiB block, no Device MAIR attribute).
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
            return MapPage(physBase, hhdm);
        }
    }

    // ── Core mapping logic ──────────────────────────────────────────

    /// <summary>
    /// Walks TTBR1 to the L2 slot covering <paramref name="physBase"/> and
    /// installs a 2 MiB Device block there unless one is already present.
    /// Silent when nothing changes: drivers call this for both ends of every
    /// region they touch, so the already-mapped path is the common one.
    /// Caller holds <see cref="s_lock"/>.
    /// </summary>
    private static bool MapPage(ulong physBase, ulong hhdmOffset)
    {
        // 2MiB-align
        ulong aligned = physBase & BLOCK_2MB_ADDR_MASK;
        ulong virtAddr = aligned + hhdmOffset;

        // ── Find Device memory MAIR index ────────────────────────
        ulong mair = DeviceMapperNative.ReadMair();
        int deviceIdx = FindDeviceMairIndex(mair);
        if (deviceIdx < 0)
        {
            Serial.Write("[DeviceMapper] ERROR: No Device MAIR index found!\n");
            return false;
        }

        // ── Read TTBR1 and walk page tables ──────────────────────
        ulong ttbr1Phys = DeviceMapperNative.ReadTtbr1() & ADDR_MASK;
        ulong* l0 = (ulong*)(ttbr1Phys + hhdmOffset);

        // L0 index (bits [47:39] of the VA offset within TTBR1 space)
        // For TTBR1 with T1SZ=16: VA bits [47:0] are used.
        // The HHDM offset is typically 0xFFFF000000000000, so
        // virtAddr - hhdmOffset = physBase. We index using physBase bits.
        int l0idx = (int)((aligned >> 39) & 0x1FF);
        ulong l0entry = l0[l0idx];

        if ((l0entry & DESC_VALID) == 0)
        {
            WriteEntryError("L0", l0idx, l0entry, "invalid", aligned);
            return false;
        }
        if ((l0entry & DESC_TABLE) == 0)
        {
            WriteEntryError("L0", l0idx, l0entry, "is block (unexpected)", aligned);
            return false;
        }

        // Follow L0 table → L1
        ulong* l1 = (ulong*)((l0entry & ADDR_MASK) + hhdmOffset);
        int l1idx = (int)((aligned >> 30) & 0x1FF);
        ulong l1entry = l1[l1idx];

        ulong* l2;

        if ((l1entry & DESC_VALID) == 0)
        {
            WriteEntryError("L1", l1idx, l1entry, "invalid", aligned);
            return false;
        }
        else if ((l1entry & DESC_TABLE) != 0)
        {
            // Table descriptor → follow to L2
            l2 = (ulong*)((l1entry & ADDR_MASK) + hhdmOffset);
        }
        else
        {
            // Block descriptor (1GiB) → need to split into L2 table
            Serial.Write("[DeviceMapper] L1 is 1GiB block, splitting...\n");
            l2 = SplitL1Block(l1, l1idx, l1entry, hhdmOffset);
            if (l2 == null)
            {
                Serial.Write("[DeviceMapper] ERROR: Failed to split L1 block\n");
                return false;
            }
        }

        // ── Write L2 entry ───────────────────────────────────────
        int l2idx = (int)((aligned >> 21) & 0x1FF);
        ulong l2entry = l2[l2idx];

        // Check if existing mapping already has Device attributes
        if ((l2entry & DESC_VALID) != 0)
        {
            int existingIdx = (int)((l2entry >> 2) & 0x7);
            byte existingAttr = (byte)((mair >> (existingIdx * 8)) & 0xFF);
            if (existingAttr == 0x00 || existingAttr == 0x04)
            {
                return true;
            }
            Serial.Write("[DeviceMapper] L2 valid but Normal memory (MAIR attr=0x");
            Serial.WriteHex(existingAttr);
            Serial.Write("), BBM replacing with Device\n");

            // ARM Break-Before-Make: must invalidate first, flush TLB,
            // then write the new descriptor. Cannot change attributes in-place.
            l2[l2idx] = 0;  // Step 1: invalidate
            DeviceMapperNative.DsbIsb();
            DeviceMapperNative.FlushTlb(virtAddr >> 12);  // Step 2: flush stale TLB
            DeviceMapperNative.DsbIsb();
        }

        // Build 2MiB block descriptor with Device attributes
        // Bits: Valid=1, Block(bit1=0), AttrIndx[4:2], AF[10], PXN[53], UXN[54]
        ulong desc = (aligned & BLOCK_2MB_ADDR_MASK)
                   | ((ulong)deviceIdx << 2)
                   | DESC_AF
                   | DESC_PXN
                   | DESC_UXN
                   | DESC_VALID;

        Serial.Write("[DeviceMapper] Mapping phys 0x");
        Serial.WriteHex(aligned);
        Serial.Write(" → virt 0x");
        Serial.WriteHex(virtAddr);
        Serial.Write(": L2[");
        Serial.WriteNumber((uint)l2idx);
        Serial.Write("] = 0x");
        Serial.WriteHex(desc);
        Serial.Write("\n");

        // Step 3: write new descriptor with Device attributes
        l2[l2idx] = desc;

        // Ensure descriptor is visible before use
        DeviceMapperNative.DsbIsb();

        // Final TLB flush for the new mapping
        DeviceMapperNative.FlushTlb(virtAddr >> 12);
        return true;
    }

    /// <summary>
    /// Logs a walk failure with the offending descriptor: the walk itself
    /// prints nothing, so this line is the only record of where it stopped.
    /// </summary>
    private static void WriteEntryError(string level, int index, ulong entry, string problem, ulong aligned)
    {
        Serial.Write("[DeviceMapper] ERROR: mapping phys 0x");
        Serial.WriteHex(aligned);
        Serial.Write(": ");
        Serial.Write(level);
        Serial.Write("[");
        Serial.WriteNumber((uint)index);
        Serial.Write("] = 0x");
        Serial.WriteHex(entry);
        Serial.Write(" ");
        Serial.Write(problem);
        Serial.Write("\n");
    }

    /// <summary>
    /// Splits a 1GiB L1 block descriptor into 512 × 2MiB L2 block descriptors,
    /// preserving the original attributes for all entries. Caller holds
    /// <see cref="s_lock"/>.
    /// </summary>
    private static ulong* SplitL1Block(ulong* l1, int l1idx, ulong l1entry, ulong hhdmOffset)
    {
        if (s_spareL2Used)
        {
            Serial.Write("[DeviceMapper] ERROR: Spare L2 table already used\n");
            return null;
        }

        // Get pre-allocated L2 table virtual address
        ulong l2va = DeviceMapperNative.GetSpareL2TableAddr();
        if (l2va == 0)
        {
            return null;
        }

        // Get its physical address (for the L1 table descriptor)
        ulong l2pa = DeviceMapperNative.VirtToPhys(l2va);
        if (l2pa == 0)
        {
            Serial.Write("[DeviceMapper] ERROR: Cannot translate spare L2 table VA\n");
            return null;
        }

        ulong* l2 = (ulong*)l2va;

        // Extract the 1GiB block's physical base and attributes
        ulong blockPhysBase = l1entry & BLOCK_1GB_ADDR_MASK;
        // Lower attributes [11:2] (AttrIndx, NS, AP, SH, AF, nG)
        ulong lowerAttrs = l1entry & 0xFFC;
        // Upper attributes [54:52] (PXN, UXN, Contiguous)
        ulong upperAttrs = l1entry & 0x0070000000000000UL;

        // Fill 512 entries as 2MiB blocks with the same attributes
        for (int i = 0; i < 512; i++)
        {
            ulong entryPhys = blockPhysBase + ((ulong)i << 21);
            l2[i] = entryPhys | lowerAttrs | upperAttrs | DESC_VALID; // bit1=0 → block
        }

        // Ensure all L2 entries are written before updating L1
        DeviceMapperNative.DsbIsb();

        // Replace L1 block with table descriptor pointing to L2
        // Table descriptor: PA | 0x3 (valid + table)
        l1[l1idx] = l2pa | DESC_VALID | DESC_TABLE;

        DeviceMapperNative.DsbIsb();

        // Flush entire TLB since we changed a 1GiB mapping
        // (vale1 only flushes one page; we need broader flush)
        DeviceMapperNative.FlushTlb(0); // will be followed by individual flushes if needed

        s_spareL2Used = true;

        Serial.Write("[DeviceMapper] Split L1 block into L2 table at PA 0x");
        Serial.WriteHex(l2pa);
        Serial.Write("\n");

        return l2;
    }

    /// <summary>
    /// Scans MAIR_EL1 for a Device memory attribute index.
    /// Looks for 0x00 (Device-nGnRnE) or 0x04 (Device-nGnRE).
    /// </summary>
    private static int FindDeviceMairIndex(ulong mair)
    {
        for (int i = 0; i < 8; i++)
        {
            byte attr = (byte)((mair >> (i * 8)) & 0xFF);
            if (attr == 0x00 || attr == 0x04)
            {
                return i;
            }
        }
        return -1;
    }
}
